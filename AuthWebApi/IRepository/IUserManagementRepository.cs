using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.Models.Entities;
using AuthWebApi.ViewModels.UserManagement;

namespace AuthWebApi.IRepository
{
    public interface IUserManagementRepository
    {
        Task<Ban> BanUserAsync(UserBanViewModel banViewMode, string adminId);
        Task UnBanUser(string userId);
        Task<bool> CheckIfUserIsBanned(string userId);
        Task<List<Ban>> GetBans();
        Task<BanDisplayModel> GetThisUserBan(string userId);
    }
}
