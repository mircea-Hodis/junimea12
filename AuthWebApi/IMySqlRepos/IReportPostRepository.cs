using System.Threading.Tasks;
using AuthWebApi.Models.Posts;

namespace AuthWebApi.IMySqlRepos
{
    public interface IReportPostRepository
    {
        Task<int> ReportPost(PostReport report);
    }
}
