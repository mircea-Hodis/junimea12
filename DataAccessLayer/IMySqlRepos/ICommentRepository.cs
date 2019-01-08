using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Comments;

namespace AuthWebApi.IMySqlRepos
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<List<Comment>> GetPostComments(int postId);
        Task<Comment> AddCommentFiles(Comment comment);
    }
}
