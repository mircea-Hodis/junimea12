using System.Collections.Generic;
using Dapper;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using AuthWebApi.Models.Comments;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AuthWebApi.MySqlRepos
{
    public class CommentRepository : ICommentRepository
    {
        private readonly string _connectionString;

        public CommentRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                comment.Id = await connection.ExecuteAsync(
                    string.Concat(
                        "INSERT INTO juniro.comments(",
                        "Message,",
                        "Likes,",
                        "PostId,",
                        "UserId)",
                        " Values(",
                        $"@{nameof(comment.Message)}, ",
                        $"@{nameof(comment.Likes)}, ",
                        $"@{nameof(comment.PostId)}, ",
                        $"@{nameof(comment.UserId)}",
                        ")",
                        "; SELECT LAST_INSERT_ID();"), comment);
            }
            return await AddPostFiles(comment);
        }

        private async Task<Comment> AddPostFiles(Comment comment)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                foreach (var commentFile in comment.Files)
                {
                    commentFile.Id = await connection.ExecuteAsync(string.Concat(
                        "INSERT INTO juniro.comment_files(",
                        "CommentId,",
                        "Url)",
                        " Values(",
                        $"@{nameof(commentFile.CommentId)}, ",
                        $"@{nameof(commentFile.Url)} ",
                        ")",
                        "; SELECT LAST_INSERT_ID();"), commentFile);
                }
            }

            return comment;
        }
    }
}
