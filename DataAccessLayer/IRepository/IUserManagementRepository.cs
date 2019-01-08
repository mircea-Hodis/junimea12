using System.Collections.Generic;
using System.Threading.Tasks;
using DataModelLayer.Models.Entities;
using DataModelLayer.ViewModels.UserManagement;

namespace DataAccessLayer.IRepository
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
