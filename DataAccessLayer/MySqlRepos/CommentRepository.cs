using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using Dapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Comments;
using DataModelLayer.Models.Entities;
using DataModelLayer.Models.Posts;
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
                    result = await GetSingleComment(updateComment.Id, connection, updateComment.UserId);
            }

            return result;
        }

        private async Task<Comment> GetSingleComment(long commentId, MySqlConnection connection, string userId)
        {
            var query = $@"SELECT
                            comments.Id,
                            comments.Message, 
                            comments.Likes, 
                            comments.PostId, 
                            comments.UserId,
                            comments.CreateDate,
                            comments.Likes,
                            AppUser.FirstName,
                            AppUser.LastName,
                            AppUser.FacebookId
                        FROM juniro.comments AS comments
                            INNER JOIN
                            juniro.usercommondata as AppUser on comments.UserId = AppUser.UserId
                        WHERE comments.Id = @commentId";

            var comment = await connection.QueryFirstOrDefaultAsync<Comment>(query, new {commentId});
            comment.CurrentUserLikeStatus = await GetCurrentUserLikeStatus(userId, comment.Id, connection);
            comment.Files = await GetCommentFiles(comment.Id, connection);
            return comment;
        }

        private async Task<int> GetCurrentUserLikeStatus(string userId, long commentId, MySqlConnection connection)
        {
            return await connection.QuerySingleOrDefaultAsync<int>(
                $@"SELECT 
                    LikeCount
                    FROM juniro.CommentLikes
                    WHERE UserId = {nameof(userId)} 
                    AND CommentId = {nameof(commentId)}",
                new { userId, commentId });
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

        public async Task<CommentLike> LikeComment(CommentLike like)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {

                var likeStatus = await connection.QuerySingleOrDefaultAsync<LikeStatus>(
                                     $@"SELECT 
                                     LikeId,
                                     LikeCount
                                     FROM juniro.commentLikes
                                     WHERE UserId = {nameof(like.UserId)} 
                                    AND CommentId = {nameof(like.CommentId)}",
                                     new { like }) ?? new LikeStatus();

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

                    like.CommentLikeCount = await connection.QuerySingleOrDefaultAsync<int>(
                        $@"INSERT INTO juniro.commentLikes (
                            LikeId, 
                            LikeTime, 
                            CommentId, 
                            UserId, 
                            LikeCount) 
                        values(
                             @{nameof(like.LikeId)},
                             @{nameof(like.LikeTime)},
                             @{nameof(like.CommentId)},
                             @{nameof(like.UserId)},
                             @{nameof(like.LikeCount)})
                        ON DUPLICATE KEY UPDATE LikeCount=VALUES(LikeCount);
                        UPDATE juniro.comments 
                           SET juniro.comments.Likes = (juniro.comments.Likes + {incrementValue})
                        WHERE juniro.comments.Id = @{nameof(like.CommentId)};
                        SELECT juniro.Comments.Likes FROM juniro.Comments WHERE juniro.comments.Id = @{nameof(like.CommentId)}", like);
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


    }
}
