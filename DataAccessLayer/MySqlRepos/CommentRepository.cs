using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using Dapper;
using DataModelLayer.Models.Comments;
using DataModelLayer.Models.Entities;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace DataAccessLayer.MySqlRepos
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
                    $@"INSERT INTO juniro.comments(
                        PostId,
                        Message,
                        Likes,
                        UserId,
                        CreateDate)
                    Values(
                        @{nameof(comment.PostId)}, 
                        @{nameof(comment.Message)}, 
                        @{nameof(comment.Likes)}, 
                        @{nameof(comment.UserId)},
                        @{nameof(comment.CreateDate)});
                    SELECT LAST_INSERT_ID();",
                        comment);
            }

            return await AddUserDataToComment(comment);
        }

        public async Task<DeleteCommentResponse> DeletePost(int commentId, string callerId)
        {
            var deleteCommentQuery = $@"
                        DELETE FROM juniro.comments
                        WHERE Id=@commentId AND UserId = @callerId;";
            var deleteCommentFileQuery = $@" 
                        DELETE FROM juniro.postfiles
                        WHERE Id=@commentId;";
          
            int affectedRows;

            using (var connection = new MySqlConnection(_connectionString))
            {
                affectedRows = await connection.ExecuteAsync(deleteCommentQuery, new { postId = commentId, callerId });
                if (affectedRows > 0)
                {
                    await connection.ExecuteAsync(deleteCommentFileQuery, new { postId = commentId });
                }
            }

            return affectedRows > 0
                ? new DeleteCommentResponse
                {
                    Message = "Comment successfully deleted.",
                    Successfull = true
                }
                : new DeleteCommentResponse
                {
                    Message = "You cannot delete this comment.",
                    Successfull = false
                };
        }

        private async Task<Comment> AddUserDataToComment(Comment comment)
        {
            var userId = comment.UserId;
            var query = $@"SELECT 
                            userCommonData.FacebookId, 
                            userCommonData.FirstName, 
                            userCommonData.LastName 
                        FROM juniro.usercommondata AS userCommonData
                        WHERE userCommonData.UserId = @userId
                        ";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entityCommonData = await connection.QuerySingleAsync<EntityCommonData>(query, new {userId});
                comment.UserFirstName = entityCommonData.FirstName;
                comment.UserLastName= entityCommonData.LastName;
                comment.FacebookId = entityCommonData.FacebookId;
            }

            return comment;
        }

        public async Task<Comment> AddCommentFiles(Comment comment)
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
            var query = $@"SELECT
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId
                        FROM juniro.comments AS comments
                        WHERE comments.UpdatedPostId = @postId
                        LIMIT 10";

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
