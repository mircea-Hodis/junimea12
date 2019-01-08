using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IRepository;
using DataModelLayer.Models.Comments;
using DataModelLayer.Models.Entities;
using DataModelLayer.Models.Posts;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace DataAccessLayer.MySqlRepos
{
    public class PostRepository : IPostRepository
    {
        private readonly string _connectionString; 

        public PostRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task<Post> CreateAsync(Post post)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                post.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.Posts(
                        CreatedDate,
                        Description, 
                        Likes,
                        PostTtile,
                        UserId)
                    VALUES (
                        @{nameof(post.CreatedDate)},
                        @{nameof(post.Description)},
                        @{nameof(post.Likes)},
                        @{nameof(post.PostTtile)},
                        @{nameof(post.UserId)});
                    SELECT LAST_INSERT_ID();", 
                        post);
            }
            return await AddUserDataToPost(post);
        }

        public async Task<int> UpdatePostAsync(UpdatePost updatePost)
        {
            int affectedRows;
            var query = $@"UPDATE juniro.posts
                    SET 
                    Description = '{updatePost.Description}',
                    PostTtile = '{updatePost.PostTtile}'
                    WHERE Id = {updatePost.Id} AND UserId = '{updatePost.UserId}';";
            using (var connection = new MySqlConnection(_connectionString))
            {
                affectedRows = await connection.ExecuteAsync(query, new {updatePost});
            }

            return affectedRows;
        }

        public async Task<DeletePostResponse> DeletePost(int postId, string callerId)
        {
            var deletePostQuery = $@"
                        DELETE FROM juniro.posts
                        WHERE Id=@postId AND UserId = @callerId;";
            var deletePostFilesQuery = $@" 
                        DELETE FROM juniro.postfiles
                        WHERE Id=@postId;";
            var deletePostLikes = $@" 
                        DELETE FROM juniro.postLikes
                        WHERE Id=@postId;";
            int affectedRows;

            using (var connection = new MySqlConnection(_connectionString))
            {
                affectedRows = await connection.ExecuteAsync(deletePostQuery, new { postId, callerId });
                if (affectedRows > 0)
                {
                    await connection.ExecuteAsync(deletePostFilesQuery, new { postId });
                    await connection.ExecuteAsync(deletePostLikes, new { postId });
                }
            }

            return affectedRows > 0
                ? new DeletePostResponse
                {
                    Message = "Post successfully deleted.",
                    Successfull = true
                }
                : new DeletePostResponse
                {
                    Message = "You cannot delete this post.",
                    Successfull = false
                };
        }

        private async Task<Post> AddUserDataToPost(Post post)
        {
            var userId = post.UserId;
            var query = $@"SELECT 
                            userCommonData.FacebookId, 
                            userCommonData.FirstName, 
                            userCommonData.LastName 
                        FROM juniro.usercommondata AS userCommonData
                        WHERE userCommonData.UserId = @userId
                        ";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entityCommonData = await connection.QueryFirstOrDefaultAsync<EntityCommonData>(query, new { userId });
                post.FirstName = entityCommonData.FirstName;
                post.LastName = entityCommonData.LastName;
                post.UserFacebookId = entityCommonData.FacebookId;
            }

            return post;
        }

        public async Task<List<Post>> GetUserPosts(string userId, DateTime startDate)
        {
            List<Post> result;
            string query = $@"SELECT posts.Id,
                                posts.PostTtile,
                                posts.Description,
                                posts.Likes,
                                posts.CreatedDate,
                                posts.UserId,
                                usercommondata.FacebookId,
                                usercommondata.FirstName,
                                usercommondata.LastName
                            FROM juniro.posts as posts
                            INNER JOIN juniro.usercommondata AS usercommondata
                            ON posts.UserId = usercommondata.UserId
                            WHERE posts.UserId = @userId && posts.CreatedDate < @startDate
                            ORDER BY posts.CreatedDate DESC
                            LIMIT 30
                          ";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new {userId , startDate});
                result = posts.AsList();
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<List<Post>> GetList(DateTime startTime, string userId)
        {
            List<Post> result;
            string query = $@"SELECT 
                            Posts.Id,
                            Posts.CreatedDate,
                            Posts.Description,
                            Posts.Likes = (
                                SELECT LikeCount FROM juniro.postlikes AS PostLikes
                                WHERE PostLikes.UserId = @userId AND Posts.Id = PostLikes.PostId),
                            Posts.PostTtile,
                            Posts.UserId,
                            AppUser.FacebookId,
                            AppUser.FirstName,
                            AppUser.LastName
                            FROM juniro.posts
                            INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId
                            WHERE CreatedDate < @startTime
                            ORDER BY CreatedDate DESC";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { startTime , userId});
                result = posts.AsList();
                
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<Post> GetPostById(int postId)
        {
            Post result;
            var query = $@"SELECT
                           Posts.Id,
                           Posts.CreatedDate,
                           Posts.Description,
                           Posts.Likes = (
                                SELECT LikeCount FROM juniro.postlikes AS PostLikes
                                WHERE Posts.Id = PostLikes.PostId),
                           Posts.PostTtile,
                           Posts.UserId, 
                           AppUser.FacebookId,
                           AppUser.FirstName,
                           AppUser.LastName
                          FROM juniro.Posts AS Posts
                              INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId 
                          WHERE Posts.Id = @postId";
            using (var connection = new MySqlConnection(_connectionString))
            {
                result = await connection.QuerySingleAsync<Post>(query, new { postId });
            }

            result = await AddPostFilesToPost(result);

            return await AddCommentsToPost(result);
        }

        private async Task<Post> AddCommentsToPost(Post post)
        {
            var postId = post.Id;
            var query = $@"SELECT
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId
                        FROM juniro.comments AS comments
                        WHERE comments.PostId = @postId
                        LIMIT 10";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var comments = await connection.QueryAsync<Comment>(query, new { postId });

                post.Comments = comments.AsList();
                foreach (var comment in post.Comments)
                {
                    comment.Files = await GetCommentFiles(comment.Id, connection);
                }
            }
            return post;
        }

        private async Task<List<CommentFiles>> GetCommentFiles(long commentId, MySqlConnection connection)
        {
            var query = $@"SELECT
                            files.Id, 
                            files.CommentId, 
                            files.Url
                        FROM juniro.comment_files AS files
                        WHERE files.CommentId = @commentId";

            var result = await connection.QueryAsync<CommentFiles>(query, new {commentId});

           return result.AsList();
        }

        private async Task<Post> AddPostFilesToPost(Post post)
        {
            var postId = post.Id; 
            string query = $@"SELECT Id
                          ,PostId
                          ,Url FROM juniro.postfiles WHERE PostId = @postId";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<PostFiles>(query, new { postId });
                post.Files = queryResult.AsList();
            }

            return post;
        }

        private async Task<List<Post>> AddPostFilesToPosts(List<Post> posts)
        {
            var postIds = posts.Select(post => post.Id).ToArray();
            var query = $@"SELECT Id
                          ,PostId
                          ,Url FROM juniro.PostFiles WHERE PostId IN @postIds";
            List<PostFiles> postFiles;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<PostFiles>(query, new { postIds });
                postFiles = queryResult.AsList();   
            }

            AddPostFilesById(posts, postFiles);

            return posts;
        }

        private void AddPostFilesById(List<Post> posts, List<PostFiles> postFiles)
        {
            foreach (var post in posts)
            {
                post.Files = postFiles.Where(postFile => postFile.PostId == post.Id).AsList();
            }
        }

        public async Task<PostLike> LikePost(PostLike like)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                like.PostLikesCount = await connection.QuerySingleOrDefaultAsync<int>(
                        $@"
                        DECLARE EXISTS VARCHAR(100) DEFAULT NULL;
                        SET EXISTS := (SELECT
                                PostLikes.UserId
                           FROM juniro.PostLikes 
                           WHERE 
                                PostId = @{nameof(like.PostId)} 
                           AND 
                                UserId = @{nameof(like.UserId)})
                         IF (EXISTS IS NOT NULL) THEN
BEGIN
                            INSERT INTO juniro.PostLikes(
                             LikeTime,
                             PostId,
                             UserId,
                             LikeCount)
                            VALUES (
                             @{nameof(like.LikeTime)},
                             @{nameof(like.PostId)},
                             @{nameof(like.UserId)},
                             @{nameof(like.LikeCount)}
                            )
END
                         ELSE 
                         BEGIN
                            UPDATE juniro.PostLikes
                                SET PostLikes.LikeTime = @{nameof(like.LikeTime)},
                                    LikeCount = @{nameof(like.LikeCount)}
                            WHERE juniro.PostLikes.PostId = @{nameof(like.PostId)}
                         END
                        UPDATE juniro.Posts 
                           SET juniro.Posts.Likes = juniro.Posts.Likes + @{nameof(like.LikeCount)}
                        WHERE juniro.Posts.Id = @{nameof(like.PostId)};
                        SELECT juniro.Posts.Likes FROM juniro.Posts WHERE juniro.Posts.Id = @{nameof(like.PostId)}", like);
            }
            return like;
        }
    }
}
