using System.Threading.Tasks;
using AuthWebApi.Models.Comments;

namespace AuthWebApi.IMySqlRepos
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);
    }
}
