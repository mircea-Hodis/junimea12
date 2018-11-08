using System.Threading.Tasks;
using AuthWebApi.Models.Entities;

namespace AuthWebApi.IMySqlRepos
{
    public interface IUserCommonDataRepository
    {
        Task AddUserCommonData(UserCommonData userCommonData);
    }
}
