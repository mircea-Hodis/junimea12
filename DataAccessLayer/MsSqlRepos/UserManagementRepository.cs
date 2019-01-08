using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IRepository;
using DataModelLayer.Models.Entities;
using DataModelLayer.ViewModels.UserManagement;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace DataAccessLayer.MsSqlRepos
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly string _msSqlConnectionString;

        public UserManagementRepository(IConfiguration configurationService)
        {
            _msSqlConnectionString = configurationService.GetConnectionString("UserAuthConnection");
        }

        public async Task<Ban> BanUserAsync(UserBanViewModel banViewMode, string adminId)
        {
            var ban = MapBan(banViewMode, adminId);

            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                ban.BanId = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO [dbo].[Bans](
                       [BanEnd]
                      ,[BanStart]
                      ,[BannedById]
                      ,[BannedEmail]
                      ,[BannedUserId]
                      ,[FacebookId])
                    VALUES (
                        @{nameof(ban.BanEnd)},
                        @{nameof(ban.BanStart)},
                        @{nameof(ban.BannedById)},
                        @{nameof(ban.BannedEmail)},
                        @{nameof(ban.BannedUserId)},
                        @{nameof(ban.FacebookId)}
                        );SELECT CAST(SCOPE_IDENTITY() as int)"
                    , ban);
            }

            return ban;
        }

        private string GetSelectQuery(string tableName)
        {
            return $"SELECT * FROM {tableName}";
        }

        public async Task UnBanUser(string userId)
        {
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                await connection.ExecuteAsync(
                    $@"IF EXISTS(SELECT TOP (1) 
                        [BannedUserId]
                    from [dbo].[Bans] WHERE [BannedUserId] = @userId)
                    BEGIN
                        DELETE FROM [dbo].[Bans] where [BannedUserId] = @userId
                    END", new {userId});
            }
        }

        public async Task<List<Ban>> GetBans()
        {
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                var result = await connection.QueryAsync<Ban>(GetSelectQuery("[Juni].[dbo].[Bans]"));
                return result.ToList();
            }
        }

        public async Task<BanDisplayModel> GetThisUserBan(string userId)
        {
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<BanDisplayModel>(
                    $@"SELECT TOP(1) Ban.[BanId]
                              ,Ban.[BannedUserId]
                              ,Ban.[BannedById]
                              ,Ban.[BanStart]
                              ,Ban.[BanEnd]
                              ,Ban.[FacebookId]
                              ,Ban.[BannedEmail]
                              ,BannedByUserName = (SELECT TOP(1) [UserName] FROM [dbo].[AspNetUsers] WHERE AppUser.[UpdatedPostId] = Ban.[BannedById])
                              ,AppUser.[UserName]
                              ,AppUser.[FirstName]
                              ,AppUser.[LastName]
                          FROM [dbo].[Bans] Ban
                            INNER JOIN 
                            [dbo].[AspNetUsers] AppUser ON Ban.[BannedUserId] = AppUser.[UpdatedPostId] 
                          WHERE Ban.[BannedUserId] = @userId", new {userId});
            }
        }

        public async Task<bool> CheckIfUserIsBanned(string userId)
        {
            var query = $@"IF EXISTS(
                           SELECT TOP (1) 
                                [BannedUserId]
                           from [dbo].[Bans] WHERE [BannedUserId] = @userId) 
                             BEGIN
                                SELECT CAST(1 AS BIT)
                             END
                           ELSE
                             BEGIN
                                SELECT CAST (0 AS BIT)
                             END";
            bool result;
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                result = await connection.QuerySingleOrDefaultAsync<bool>(query, new {userId});
            }

            return result;
        }   

        private static Ban MapBan(UserBanViewModel banViewModel, string adminId)
        {
            return new Ban
            {
                BannedById = adminId,
                BanStart = DateTime.Now,
                BannedEmail = banViewModel.BannedEmail,
                BannedUserId = banViewModel.BannedUserId,
                FacebookId = banViewModel.FacebookId,
                BanEnd = !banViewModel.IsPermanentBan
                    ? DateTime.Now.AddDays(banViewModel.BanWeeksDuration * 7)
                    : DateTime.MaxValue
            };
        }

    }
}
