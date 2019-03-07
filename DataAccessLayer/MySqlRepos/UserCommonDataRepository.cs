using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Entities;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace DataAccessLayer.MySqlRepos
{
    public class UserCommonDataRepository : IUserCommonDataRepository
    {
        private readonly string _connectionString;

        public UserCommonDataRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task AddUserCommonData(UserCommonData data)
        {
            var query = $@"INSERT INTO juniro.usercommondata(
                            FacebookId,
                            FirstName,
                            LastName,
                            UserEmail,
                            UserId,
                            UserLevel,
                            UserName)
                        Values(
                            @{nameof(data.FacebookId)},
                            @{nameof(data.FirstName)},
                            @{nameof(data.LastName)},
                            @{nameof(data.UserEmail)},
                            @{nameof(data.UserId)},
                            @{nameof(data.UserLevel)},
                            @{nameof(data.UserName)})";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(query, data);
            }
        }

        public async Task DeleteUserCommonData(string userId)
        {
            var query = $@"
                        DELETE FROM juniro.usercommondata
                        WHERE UserId = @userId;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(query, new {userId});
            }
        }
    }
}
