using System.Threading.Tasks;
using DataModelLayer.Models.Posts;

namespace AuthWebApi.IMySqlRepos
{
    public interface IReportPostRepository
    {
        Task<int> ReportPost(PostReport report);
    }
}
