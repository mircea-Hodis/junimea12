using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using AuthWebApi.Models.Comments;
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

            if (!comment.Files.Any())
                return comment;
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

        public async Task<List<Comment>> GetPostComments(int postId)
        {
            var query = $@"SELECT TOP (10)
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId
                        FROM juniro.comments AS comments
                        WHERE comments.PostId = @postId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<Comment>(query, new { postId });

                var comments = result.AsList();
                foreach (var comment in comments)
                {
                    comment.Files = await GetCommentFiles(comment.Id, connection);
                }
                return comments;
            }
        }

        private async Task<List<CommentFiles>> GetCommentFiles(long commentId, MySqlConnection connection)
        {
            var query = $@"SELECT
                            files.Id, 
                            files.CommentId, 
                            files.Url
                        FROM juniro.comment_files AS files
                        WHERE files.CommentId = @commentId";

            var result = await connection.QueryAsync<CommentFiles>(query, new { commentId });

            return result.AsList();
        }
    }
}
