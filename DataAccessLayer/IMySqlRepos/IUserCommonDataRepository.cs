using System.Threading.Tasks;
using DataModelLayer.Models.Entities;

namespace DataAccessLayer.IMySqlRepos
{
    public interface IUserCommonDataRepository
    {
        Task AddUserCommonData(UserCommonData userCommonData);
        Task DeleteUserCommonData(string userId);
    }
}
