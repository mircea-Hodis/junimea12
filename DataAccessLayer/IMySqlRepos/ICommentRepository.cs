using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Comments;

namespace AuthWebApi.IMySqlRepos
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> AddCommentFiles(Comment comment);
        Task<DeleteCommentResponse> DeleteComment(int commentId, string callerId);
        Task<Comment> UpdateComment(UpdateComment updateComment);
        Task UpdateCommentImages(List<CommentFiles> postFiles, long commentId);
    }
}
