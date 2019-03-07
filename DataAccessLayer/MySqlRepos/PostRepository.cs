using System;
using System.Collections.Generic;
using System.Drawing;
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
                        WHERE PostId=@postId;";
          
            int affectedRows;
            var toBeDeletedPost = await GetPostById(postId, string.Empty);
            using (var connection = new MySqlConnection(_connectionString))
            {
                affectedRows = await connection.ExecuteAsync(deletePostQuery, new { postId, callerId });
                if (affectedRows > 0)
                {
                    await connection.ExecuteAsync(deletePostFilesQuery, new { postId });
                    await connection.ExecuteAsync(deletePostLikes, new { postId });
                    await DeleteComment(toBeDeletedPost.Comments, toBeDeletedPost.Id, connection);
                }
            }

            return affectedRows > 0
                ? new DeletePostResponse
                {
                    Message = "Post successfully deleted.",
                    Successfull = true,
                    Post = toBeDeletedPost
                }
                : new DeletePostResponse
                {
                    Message = "You cannot delete this post.",
                    Successfull = false,
                    Post = toBeDeletedPost

                };
        }

        private async Task DeleteComment(List<Comment> comments, int postId, MySqlConnection connection)
        {
            var deletePostComments = $@"
                        DELETE FROM juniro.comments 
                        where PostId = @postId";
            var deletePostCommentsFiles = $@" 
                        DELETE FROM juniro.comment_files
                        WHERE CommentId=@commentId;";
            await connection.ExecuteAsync(deletePostComments, new {postId});
            await connection.ExecuteAsync(deletePostComments, new {postId});
            foreach (var comment in comments)
            {
                var commentId = comment.Id;
                await connection.ExecuteAsync(deletePostCommentsFiles, new {commentId});
            }
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
                foreach (var post in result)
                {
                    post.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, post.Id, connection);
                }
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<List<Post>> GetListInitial(string userId)
        {
            List<Post> result;
            string query = $@"SELECT 
                            Posts.Id,
                            Posts.CreatedDate,
                            Posts.Description,
                            Posts.Likes,
                            Posts.PostTtile,
                            Posts.UserId,
                            AppUser.FacebookId,
                            AppUser.FirstName,
                            AppUser.LastName
                            FROM juniro.posts
                            INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId
                            ORDER BY CreatedDate
                            Limit 5";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query);
                result = posts.AsList();
                if(!string.IsNullOrEmpty(userId))
                    foreach (var post in result)
                    {
                        post.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, post.Id , connection);
                    }
            }

            return await AddPostFilesToPosts(result);
        }

        public async Task<List<Post>> GetList(DateTime startTime, string userId)
        {
            List<Post> result;
            var query = $@"SELECT 
                            Posts.Id,
                            Posts.CreatedDate,
                            Posts.Description,
                            Posts.Likes,
                            Posts.PostTtile,
                            Posts.UserId,
                            AppUser.FacebookId,
                            AppUser.FirstName,
                            AppUser.LastName
                            FROM juniro.posts
                            INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId
                            WHERE CreatedDate < @startTime
                            ORDER BY CreatedDate";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { startTime });
                result = posts.AsList();
                if (!string.IsNullOrEmpty(userId))
                    foreach (var post in result)
                    {
                        post.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, post.Id, connection);
                    }
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<Post> GetPostById(int postId, string userId)
        {
            Post result;
            var query = $@"SELECT
                           Posts.Id,
                           Posts.CreatedDate,
                           Posts.Description,
                           Posts.Likes,
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
                result = await connection.QuerySingleOrDefaultAsync<Post>(query, new { postId });
                if(!string.IsNullOrEmpty(userId) && result != null)
                    result.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, result.Id, connection);
            }

            result = await AddPostFilesToPost(result);

            return await AddCommentsToPost(result, userId);
        }

        private async Task<int> GetCurrentUserLikeStatus(string userId, int postId, MySqlConnection connection)
        {

            return await connection.QuerySingleOrDefaultAsync<int>(
                $@"SELECT 
                      LikeCount
                      FROM juniro.PostLikes
                   WHERE UserId = '{userId}'AND PostId = {postId}");
        }

        private async Task<Post> AddCommentsToPost(Post post, string userId)
        {
            var postId = post.Id;
            var query = $@"SELECT
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId,
                            comments.CreateDate,
                            AppUser.FirstName,
                            AppUser.LastName,
                            AppUser.FacebookId
                        FROM juniro.comments AS comments
                            INNER JOIN
                            juniro.usercommondata as AppUser on comments.UserId = AppUser.UserId
                        WHERE comments.PostId = @postId
                        ORDER BY comments.CreateDate
                        LIMIT 3 
                        ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var comments = await connection.QueryAsync<Comment>(query, new { postId });

                post.Comments = comments.AsList();
                if (post.Comments.Count == 5)
                {
                    post.AreStillCommentsToGet = true;
                }
                foreach (var comment in post.Comments)
                {
                    comment.Files = await GetCommentFiles(comment.Id, connection);
                    comment.CurrentUserLikeStatus =
                        await GetCurrentUserCommentLikeStatus(userId, comment.Id, connection);
                }
            }
            return post;
        }

        private async Task<int> GetCurrentUserCommentLikeStatus(string userId, long commentId, MySqlConnection connection)
        {
            return await connection.QuerySingleOrDefaultAsync<int>(
                $@"SELECT 
                      LikeCount
                      FROM juniro.CommentLikes
                   WHERE UserId = '{userId}'AND CommentId = {commentId}");
        }

        public async Task<List<Comment>> GetRemainingComments(int postId, DateTime lastCommentDate)
        {
            var query = $@"SELECT
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId,
                            comments.CreateDate,
                            AppUser.FirstName,
                            AppUser.LastName,
                            AppUser.FacebookId
                        FROM juniro.comments AS comments
                            INNER JOIN
                            juniro.usercommondata as AppUser on comments.UserId = AppUser.UserId
                        WHERE comments.PostId = @postId AND comments.CreateDate > @lastCommentDate
                        LIMIT 10";
            List<Comment> result;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<Comment>(query, new {postId, lastCommentDate});
                result = queryResult.AsList();

                foreach (var comment in result)
                {
                    comment.Files = await GetCommentFiles(comment.Id, connection);
                }
            }

            return result;
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

                var likeStatus = await connection.QuerySingleOrDefaultAsync<LikeStatus>(
                                     $@"SELECT 
                                     LikeId,
                                     LikeCount
                                     FROM juniro.PostLikes
                                     WHERE UserId = {nameof(like.UserId)} 
                                    AND PostId = {nameof(like.PostId)}",
                                     new {like}) ?? new LikeStatus();

                var areBothPositve = likeStatus.LikeCount > 0 && like.LikeCount > 0;
                var areBothNegative = likeStatus.LikeCount < 0 && like.LikeCount < 0;
                var areDifferent = likeStatus.LikeCount != like.LikeCount;
                if (areDifferent && !areBothNegative && !areBothPositve)
                {
                    var incrementValue = GetLikeIncrementalValue(like.LikeCount, likeStatus.LikeCount);
                    like.LikeCount = incrementValue;
                    if (incrementValue == 1 && likeStatus.LikeCount < 0)
                        like.LikeCount = 0;
                    if (incrementValue == -1 && likeStatus.LikeCount > 0)
                        like.LikeCount = 0;
                    if (likeStatus.LikeId > 0)
                        like.LikeId = likeStatus.LikeId;
                    if (incrementValue < -1)
                        like.LikeCount = -1;
                    if (incrementValue > 1)
                        like.LikeCount = 1;

                    like.PostLikesCount = await connection.QuerySingleOrDefaultAsync<int>(
                        $@"INSERT INTO juniro.postLikes (
                            LikeId, 
                            LikeTime, 
                            PostId, 
                            UserId, 
                            LikeCount) 
                        values(
                             @{nameof(like.LikeId)},
                             @{nameof(like.LikeTime)},
                             @{nameof(like.PostId)},
                             @{nameof(like.UserId)},
                             @{nameof(like.LikeCount)})
                        ON DUPLICATE KEY UPDATE LikeCount=VALUES(LikeCount);
                        UPDATE juniro.Posts 
                           SET juniro.Posts.Likes = (juniro.Posts.Likes + {incrementValue})
                        WHERE juniro.Posts.Id = @{nameof(like.PostId)};
                        SELECT juniro.Posts.Likes FROM juniro.Posts WHERE juniro.Posts.Id = @{nameof(like.PostId)}", like);
                }
            }
            return like;
        }

        private int GetLikeIncrementalValue(int likeCount, int likeStatusCount)
        {
            var incrementValue = 1;
            if (likeCount < 0)
                incrementValue = -1;
            if (likeCount > 0)
                incrementValue = 1;
            if (likeStatusCount == -1 && likeCount > 0)
                incrementValue = 2;
            if (likeStatusCount == 1 && likeCount < 0)
                incrementValue = -2;
            if (likeCount == 0 && likeStatusCount > 0)
                incrementValue = -1;
            return incrementValue;
        }

        public async Task<Post> GetNextPost(int currentId, string userId)
        {
            Post result;
            var query = $@"SELECT
                           Posts.Id,
                           Posts.CreatedDate,
                           Posts.Description,
                           Posts.Likes,
                           Posts.PostTtile,
                           Posts.UserId, 
                           AppUser.FacebookId,
                           AppUser.FirstName,
                           AppUser.LastName
                          FROM juniro.Posts AS Posts
                              INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId 
                         WHERE Posts.Id > @currentId";
            using (var connection = new MySqlConnection(_connectionString))
            {
                result = await connection.QueryFirstOrDefaultAsync<Post>(query, new { currentId });
                if (!string.IsNullOrEmpty(userId) && result != null)
                    result.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, result.Id, connection);
            }

            result = await AddPostFilesToPost(result);

            return await AddCommentsToPost(result, userId);
        }

        public async Task<Post> GetPrevious(int currentId, string userId)
        {
            Post result;
            var query = $@"SELECT
                           Posts.Id,
                           Posts.CreatedDate,
                           Posts.Description,
                           Posts.Likes,
                           Posts.PostTtile,
                           Posts.UserId, 
                           AppUser.FacebookId,
                           AppUser.FirstName,
                           AppUser.LastName
                          FROM juniro.Posts AS Posts
                              INNER JOIN 
                              juniro.usercommondata AS AppUser ON Posts.UserId = AppUser.UserId 
                          WHERE Posts.Id < @currentId ORDER BY Posts.Id DESC LIMIT 1";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<Post>(query, new { currentId });
                result = queryResult.FirstOrDefault();
                if (result == null)
                    return null;
                if (!string.IsNullOrEmpty(userId) && result != null)
                    result.CurrentUserLikeValue = await GetCurrentUserLikeStatus(userId, result.Id, connection);
            }

            result = await AddPostFilesToPost(result);

            return await AddCommentsToPost(result, userId);
        }
        
    }
}
