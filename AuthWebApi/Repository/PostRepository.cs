using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using AuthWebApi.Models.Posts;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DataManagement.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly string _connectionString;

        public PostRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("DefaultConnection");
        }

        public async Task<Post> CreateAsync(Post post)
        {
            using (var connection = new SqlConnection(_connectionString))
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
            var result = new List<Post>();
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
            using (var connection = new SqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { userId });
                result = posts.AsList();

            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<List<Post>> GetList(DateTime startTime)
        {
            var result = new List<Post>();
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
                          WHERE [CreatedDate] < @startTime";
            using (var connection = new SqlConnection(_connectionString))
            {
                var posts = await connection.QueryAsync<Post>(query, new { startTime });
                result = posts.AsList();
                
            }
            return await AddPostFilesToPosts(result);
        }

        public async Task<Post> GetPostById(int postId)
        {
            var result = new Post();
            string query = $@"SELECT TOP (30) 
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
            using (var connection = new SqlConnection(_connectionString))
            {
                result = await connection.QuerySingleAsync<Post>(query, new { postId });
            }

            return await AddPostFilesToPost(result);
        }

        private async Task<Post> AddPostFilesToPost(Post post)
        {
            var postId = post.Id; 
            string query = $@"SELECT [Id]
                          ,[PostId]
                          ,[Url] FROM [dbo].[PostFiles] WHERE [PostId] = @postId";
            using (var connection = new SqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<PostFiles>(query, new { postId });
                post.Files = queryResult.AsList();
            }

            return post;
        }

        private async Task<List<Post>> AddPostFilesToPosts(List<Post> posts)
        {
            var postIds = posts.Select(post => post.Id).ToArray();
            string query = $@"SELECT [Id]
                          ,[PostId]
                          ,[Url] FROM [dbo].[PostFiles] WHERE [PostId] IN @postIds";
            var postFiles = new List<PostFiles>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var queryResult = await connection.QueryAsync<PostFiles>(query, new { postIds });
                postFiles = queryResult.AsList();
            }

            foreach(var post in posts)
            {
                post.Files = postFiles.Where(postFile => postFile.PostId == post.Id).ToList();
            }

            return posts;
        }
    }
}
