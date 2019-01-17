using System;
using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Entities;
using DataModelLayer.Models.Tikets;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace DataAccessLayer.MySqlRepos
{
    public class TicketsRepository : ITicketsRepository
    {
        private readonly string _mysqlConnectionString;

        public TicketsRepository(IConfiguration configurationService)
        {
            _mysqlConnectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task<Ticket> CreateTicket(Ticket ticket, string callerId)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                ticket.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.ticket(
                        TicketIssuerUserId,
                        Message,
                        IsPending,
                        IsAddressed,
                        AddressedMessage,
                        AddressedById,
                        CreatedDate)
                    VALUES (
                        @{nameof(ticket.TicketIssuerUserId)},
                        @{nameof(ticket.Message)},
                        @{nameof(ticket.IsPending)},
                        @{nameof(ticket.IsAddressed)},
                        @{nameof(ticket.AddressedMessage)},
                        @{nameof(ticket.AddressedById)},
                        @{nameof(ticket.CreatedDate)});
                    SELECT LAST_INSERT_ID();",
                    ticket);
                ticket.CommonData = await AddUserDataToTicket(ticket, connection);
            }
            
            return ticket;
        }

        private async Task<EntityCommonData> AddUserDataToTicket(Ticket ticket, MySqlConnection connection)
        {
            var userId = ticket.TicketIssuerUserId;
            var query = $@"SELECT 
                            userCommonData.FacebookId, 
                            userCommonData.FirstName, 
                            userCommonData.LastName 
                        FROM juniro.usercommondata AS userCommonData
                        WHERE userCommonData.UserId = @userId
                        ";
            return await connection.QueryFirstOrDefaultAsync<EntityCommonData>(query, new { userId });
        }

        public async Task<bool> AddressTicket(AddressTicketViewModel model, DateTime stagedDate, string stagedByUserId)
        {
            int result;
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                var query = $@"Update juniro.ticket
                               Set
                               IsPending = {false},
                               IsAddressed = {true},
                               AddressedMessage = '{model.Message}',
                               AddressedById = '{stagedByUserId}'
                               Where Id = {model.TicketId};";
                result = await connection.ExecuteAsync(query, new { model});
            }

            return result > 0;
        }

        public async Task<int> ReportEntity(ReportEntity reportEntity)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                reportEntity.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.Posts(
                        CreatedDate,
                        IsAddresed, 
                        AddresedMessage,
                        Message,
                        TicketIssuerUserId,
                        EntityId,
                        ReportedEntityId)
                    VALUES (
                        @{nameof(reportEntity.CreatedDate)},
                        @{nameof(reportEntity.IsAddressed)},
                        @{nameof(reportEntity.AddressedMessage)},
                        @{nameof(reportEntity.Message)},
                        @{nameof(reportEntity.ReportedByUserId)});
                        @{nameof(reportEntity.ReportedEntityId)});
                    SELECT LAST_INSERT_ID();",
                    reportEntity);
            }
            return reportEntity.Id;
        }
    }
}
