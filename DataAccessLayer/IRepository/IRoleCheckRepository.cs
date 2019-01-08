using System.Threading.Tasks;

namespace AuthWebApi.IRepository
{
    public interface IRoleCheckRepository
    {
        Task<int> GetUserRole(string userId);
    }
}
