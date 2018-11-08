using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.Models.Comments;

namespace AuthWebApi.IMySqlRepos
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<List<Comment>> GetPostComments(int postId);
    }
}
