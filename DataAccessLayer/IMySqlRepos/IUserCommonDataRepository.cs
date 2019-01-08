using System.Threading.Tasks;
using DataModelLayer.Models.Entities;

namespace AuthWebApi.IMySqlRepos
{
    public interface IUserCommonDataRepository
    {
        Task AddUserCommonData(UserCommonData userCommonData);
    }
}
