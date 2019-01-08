using System.Data.SqlClient;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.MsSqlRepos
{
    public class RoleCheckRepository : IRoleCheckRepository
    {
        private readonly string _msSqlConnectionString;

        public RoleCheckRepository(IConfiguration configurationService)
        {
            _msSqlConnectionString = configurationService.GetConnectionString("UserAuthConnection");
        }

        public async Task<int> GetUserRole(string userId)
        {
            var query = $@"SELECT [RoleId] FROM [dbo].[UserHighRoles] WHERE [UserId] = @userId";
            using (var connection = new SqlConnection(_msSqlConnectionString))
            {
                var queryResult = await connection.QuerySingleOrDefaultAsync<int>(query, new {userId});
                if (queryResult > 0)
                    return queryResult;
            }

            return 0;
        }
    }
}
