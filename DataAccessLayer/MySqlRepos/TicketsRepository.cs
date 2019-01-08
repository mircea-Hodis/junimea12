using System;
using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Tikets;
using Microsoft.Extensions.Configuration;
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

        public async Task<int> CreateTicket(Ticket ticket, string callerId)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                ticket.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.Posts(
                        CreatedDate,
                        Message,
                        TicketIssuerUserId)
                    VALUES (
                        @{nameof(ticket.CreatedDate)},
                        @{nameof(ticket.Message)},
                        @{nameof(ticket.TicketIssuerUserId)}
                    SELECT LAST_INSERT_ID();",
                    ticket);
            }
            return ticket.Id;
        }

        public async Task<Ticket> AddFilesToTicket(Ticket ticket)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                foreach (var file in ticket.TicketFiles)
                {
                    file.Id = await connection.ExecuteAsync(string.Concat(
                        "INSERT INTO juniro.ticket_files(",
                        "CommentId,",
                        "Url)",
                        " Values(",
                        $"@{nameof(file.Id)}, ",
                        $"@{nameof(file.Url)} ",
                        ")",
                        "; SELECT LAST_INSERT_ID();"), file);
                }
            }
            return ticket;
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

        public async Task<ReportEntity> AddFilesToReportEntity(ReportEntity reportEntity)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                foreach (var file in reportEntity.ReportFiles)
                {
                    file.Id = await connection.ExecuteAsync(string.Concat(
                        "INSERT INTO juniro.ticket_files(",
                        "ReportEntityId,",
                        "Url)",
                        " Values(",
                        $"@{nameof(file.ReportEntityId)}, ",
                        $"@{nameof(file.Url)} ",
                        ")",
                        "; SELECT LAST_INSERT_ID();"), file);
                }
            }
            return reportEntity;
        }
    }
}
