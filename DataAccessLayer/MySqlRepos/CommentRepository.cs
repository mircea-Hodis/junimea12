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
                comment.Id = await connection.QuerySingleAsync<int>(
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

        public async Task<Comment> UpdateComment(UpdateComment updateComment)
        {
            var query = $@"UPDATE juniro.comments
                    SET 
                    Message = '{updateComment.Comment}'
                    WHERE Id = {updateComment.Id} AND UserId = '{updateComment.UserId}';";
            var result = new Comment();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var affectedRows = await connection.ExecuteAsync(query, new { updateComment });
                if (affectedRows > 0)
                    result = await GetSingleComment(updateComment.Id, connection);
            }

            return result;
        }

        private async Task<Comment> GetSingleComment(long commentId, MySqlConnection connection)
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
                        WHERE comments.Id = @commentId";

            var comment = await connection.QueryFirstOrDefaultAsync<Comment>(query, new {commentId});
            comment.Files = await GetCommentFiles(comment.Id, connection);
            return comment;
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

        public async Task UpdateCommentImages(List<CommentFiles> postFiles, long commentId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync($@"DELETE FROM juniro.comment_files WHERE CommentId = {commentId}");

                foreach (var file in postFiles)
                {
                    file.Id = await connection.QuerySingleAsync<int>(
                        $@"INSERT INTO juniro.comment_files(
                                CommentId, 
                                Url)
                            VALUES (
                                @{nameof(file.CommentId)},
                                @{nameof(file.Url)});
                            SELECT LAST_INSERT_ID();"
                        , file);
                }
            }
        }

        public async Task<DeleteCommentResponse> DeleteComment(int commentId, string callerId)
        {
            var deleteCommentQuery = $@"
                        DELETE FROM juniro.comments
                        WHERE Id=@commentId AND UserId = @callerId;";
            var deleteCommentFileQuery = $@" 
                        DELETE FROM juniro.comment_files
                        WHERE CommentId=@commentId;";
            var toBeDeletedCommentFiles = $@"
                        SELECT * FROM juniro.comment_files
                        WHERE CommentId=@commentId";
            var result = new DeleteCommentResponse()
            {
                Message = "You cannot delete this comment.",
                Successfull = false,
                RemainingFiles = new List<CommentFiles>()
            };
            using (var connection = new MySqlConnection(_connectionString))
            {
                var affectedRows = await connection.ExecuteAsync(deleteCommentQuery, new { commentId, callerId });

                if (affectedRows <= 0) return result;

                var files = await connection.QueryAsync<CommentFiles>(toBeDeletedCommentFiles, new { commentId });
                await connection.ExecuteAsync(deleteCommentFileQuery, new { commentId });

                result.RemainingFiles = files.AsList();
                result.Message = "Comment successfully deleted.";
                result.Successfull = true;
            }

            return result;
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
                comment.FirstName = entityCommonData.FirstName;
                comment.LastName= entityCommonData.LastName;
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

     
   
    }
}
