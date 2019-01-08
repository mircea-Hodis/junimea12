using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using Dapper;
using DataModelLayer.Models.Posts;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AuthWebApi.MySqlRepos
{
    public class ReportPostRepository : IReportPostRepository
    {
        private readonly string _connectionString;

        public ReportPostRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("MysqlConnection");
        }
        public async Task<int> ReportPost(PostReport report)
        {
            var query = $@"
                        SELECT UpdatedPostId FROM juniro.postreports 
                            WHERE 
                                ReportById = @{nameof(report.ReportById)} 
                                AND
                                UpdatedPostId = @{nameof(report.PostId)};
                        set @count = found_rows();
                        IF @count > 0
                        THEN
                            INSERT INTO juniro.postreports(
                                UpdatedPostId,
                                Reason,
                                ReportById,
                                ReportTime)
                            Values(
                                @{nameof(report.PostId)},
                                @{nameof(report.Reason)},
                                @{nameof(report.ReportById)},
                                @{nameof(report.ReportTime)});
                            SELECT LAST_INSERT_ID();
                        ELSE 
                        SELECT 0
                        END IF";
            using (var connection = new MySqlConnection(_connectionString))
            {
                report.Id = await connection.ExecuteAsync(query, report);
            }

            return report.Id;
        }
    }
}
