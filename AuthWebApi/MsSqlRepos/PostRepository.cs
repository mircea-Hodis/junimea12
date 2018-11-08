using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using AuthWebApi.Models.Comments;
using AuthWebApi.Models.Posts;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AuthWebApi.MsSqlRepos
{
    public class PostRepository : IPostRepository
    {
        private readonly string _msSqlConnectionString;
        private readonly string _mysqlConnectionString; 

        public PostRepository(IConfiguration configurationService)
        {
            _msSqlConnectionString = configurationService.GetConnectionString("UserAuthConnection");
            _mysqlConnectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task<Post> CreateAsync(Post post)
        {
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                post.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO [dbo].[Posts](
                        [CreatedDate],
                        [Description], 
                        [Likes],
                        [PostTtile],
                        [UserId])
                    VALUES (
                        @{nameof(post.CreatedDate)},
                        @{nameof(post.Description)},
                        @{nameof(post.Likes)},
                        @{nameof(post.PostTtile)},
                        @{nameof(post.UserId)}
                        );SELECT CAST(SCOPE_IDENTITY() as int)"
                       , post);
            }
            return post;
        }

        public async Task<List<Post>> GetUserPosts(string userId)
        {
            List<Post> result;
            string query = $@"SELECT TOP (30) 
                           Posts.[Id]
                          ,Posts.[CreatedDate]
                          ,Posts.[Description]
                          ,Posts.[Likes]
                          ,Posts.[PostTtile]
                          ,Posts.[UserId]
                          ,AppUser.[FacebookId]
                          ,AppUser.[FirstName] 
                          ,AppUser.[LastName]
                          FROM [dbo].[Posts] Posts 
                              INNER JOIN 
                              [dbo].[AspNetUsers] AppUser ON Posts.[UserId] = AppUser.[Id]
                          WHERE [UserId] = @userId";
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { userId });
                result = posts.AsList();

            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<List<Post>> GetList(DateTime startTime, string userId)
        {
            List<Post> result;
            string query = $@"SELECT TOP (30) 
                           Posts.[Id]
                          ,Posts.[CreatedDate]
                          ,Posts.[Description]
                          ,Posts.[Likes]
                          ,Posts.[PostTtile]
                          ,Posts.[UserId]
                          ,AppUser.[FacebookId]
                          ,AppUser.[FirstName] 
                          ,AppUser.[LastName]
                          ,LikeCount(
                                SELECT [LikeCount] FROM [dbo].[PostLikes] PostLikes
                                WHERE PostLikes.[UserId] = @userId AND Posts.[Id] = PostLikes.[PostId])
                          FROM [dbo].[Posts] Posts 
                              INNER JOIN 
                              [dbo].[AspNetUsers] AppUser ON Posts.[UserId] = AppUser.[Id]
                          WHERE [CreatedDate] < @startTime";
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { startTime , userId});
                result = posts.AsList();
                
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<Post> GetPostById(int postId)
        {
            Post result;
            var query = $@"SELECT TOP (1) 
                           Post.[Id]
                          ,Post.[CreatedDate]
                          ,Post.[Description]
                          ,Post.[Likes]
                          ,Post.[PostTtile]
                          ,Post.[UserId] 
                          ,AppUser.[FacebookId]
                          ,AppUser.[FirstName] 
                          ,AppUser.[LastName]
                          FROM [dbo].[Posts] Post
                              INNER JOIN 
                              [dbo].[AspNetUsers] AppUser ON Post.[UserId] = AppUser.[Id] 
                          WHERE Post.[Id] = @postId";
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                result = await connection.QuerySingleAsync<Post>(query, new { postId });
            }

            result = await AddPostFilesToPost(result);

            return await AddCommentsToPost(result);
        }

        private async Task<Post> AddCommentsToPost(Post post)
        {
            var postId = post.Id;
            var query = $@"SELECT TOP (10)
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId
                        FROM juniro.comments AS comments
                        WHERE comments.PostId = @postId";

            using (var connection = new MySqlConnection(_mysqlConnectionString))
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
            string query = $@"SELECT [Id]
                          ,[PostId]
                          ,[Url] FROM [dbo].[PostFiles] WHERE [PostId] = @postId";
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                var queryResult = await connection.QueryAsync<PostFiles>(query, new { postId });
                post.Files = queryResult.AsList();
            }

            return post;
        }

        private async Task<List<Post>> AddPostFilesToPosts(List<Post> posts)
        {
            var postIds = posts.Select(post => post.Id).ToArray();
            var query = $@"SELECT [Id]
                          ,[PostId]
                          ,[Url] FROM [dbo].[PostFiles] WHERE [PostId] IN @postIds";
            List<PostFiles> postFiles;
            using (var connection = new SqlConnection(_msSqlConnectionString))
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
                post.Files = postFiles.Where(postFile => postFile.PostId == post.Id).ToList();
            }
        }

        public async Task<PostLike> LikePost(PostLike like)
        {
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                like.PostLikesCount = await connection.QuerySingleOrDefaultAsync<int>(
                        $@"IF NOT EXISTS( 
                            SELECT TOP (1) 
                            [UserId] 
                            FROM [dbo].[PostLikes] 
                            WHERE [PostId] = @{nameof(like.PostId)} AND [UserId] = @{nameof(like.UserId)})
                         BEGIN
                            INSERT INTO [dbo].[PostLikes](
                             [LikeTime]
                            ,[PostId]
                            ,[UserId]
                            ,[LikeCount])
                            VALUES (
                             @{nameof(like.LikeTime)}
                            ,@{nameof(like.PostId)}
                            ,@{nameof(like.UserId)}
                            ,@{nameof(like.LikeCount)}
                            )
                            UPDATE [dbo].[Posts] 
                                SET [Posts].[Likes] = [Posts].[Likes] + @{nameof(like.LikeCount)}
                            WHERE [dbo].[Posts].[Id] = @{nameof(like.PostId)}
                            ;SELECT CAST([Posts].[Likes] as int) FROM [dbo].[Posts] WHERE [dbo].[Posts].[Id] = @{nameof(like.PostId)}
                        END
                        ELSE 
                        BEGIN
                            UPDATE [dbo].[PostLikes]
                                SET [LikeTime] = @{nameof(like.LikeTime)},
                                    [LikeCount] = @{nameof(like.LikeCount)}
                            WHERE [dbo].[PostLikes].[PostId] = @{nameof(like.PostId)}
                            UPDATE [dbo].[Posts] 
                                SET [Posts].[Likes] = [Posts].[Likes] + @{nameof(like.LikeCount)}
                            WHERE [dbo].[Posts].[Id] = @{nameof(like.PostId)}
                            ;SELECT CAST([Posts].[Likes] as int) FROM [dbo].[Posts] WHERE [dbo].[Posts].[Id] = @{nameof(like.PostId)}
                        END", like);
            }
            return like;
        }
    }
}
