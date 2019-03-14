using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Entities;
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

        public async Task<int> ReportPost(PostReport reportEntity)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                var queryResult = await connection.QueryAsync<CommentReport>(@"SELECT * FROM juniro.commentreports 
                                                                        where ReportedByUserId = @userId");

                reportEntity.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.PostReports(
                        EntityId,
                        Message,
                        ReportedByUserId,
                        CreatedDate)
                    VALUES (
                        @{nameof(reportEntity.EntityId)},
                        @{nameof(reportEntity.Message)},
                        @{nameof(reportEntity.AddressedMessage)},
                        @{nameof(reportEntity.CreatedDate)});
                    SELECT LAST_INSERT_ID();",
                    reportEntity);
            }
            return reportEntity.Id;
        }

        public async Task<int> AddressPostReport(PostReport postReport)
        {
            var query = $@"UPDATE juniro.PostReports
                           SET 
                               AddressedById = '{postReport.AddressedById}',
                               AddressedMessage = '{postReport.AddressedMessage}',
                               AddresDateTime = '{postReport.AddresDateTime}'
                           WHERE Id = {postReport.Id}";

            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                return await connection.ExecuteAsync(query, new { postReport });
            }
        }

        public async Task<int> ReportComment(CommentReport reportEntity)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                reportEntity.Id = await connection.QuerySingleAsync<int>(
                    $@"INSERT INTO juniro.CommentReports(
                        EntityId,
                        Message,
                        ReportedByUserId,
                        CreatedDate)
                    VALUES (
                        @{nameof(reportEntity.EntityId)},
                        @{nameof(reportEntity.Message)},
                        @{nameof(reportEntity.AddressedMessage)},
                        @{nameof(reportEntity.CreatedDate)});
                    SELECT LAST_INSERT_ID();",
                    reportEntity);
            }
            return reportEntity.Id;
        }

        public async Task<int> AddressCommentReport(CommentReport postReport)
        {
            var query = $@"UPDATE juniro.CommentReports
                           SET 
                               AddressedById = '{postReport.AddressedById}',
                               AddressedMessage = '{postReport.AddressedMessage}',
                               AddresDateTime = '{postReport.AddresDateTime}'
                           WHERE Id = {postReport.Id} AND PostId = '{postReport.PostId}";

            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                return await connection.ExecuteAsync(query, new { postReport });
            }
        }

        public async Task<List<Ticket>> GetUserTicket(string userId)
        {
            var result = new List<Ticket>();
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                var queryResult = await connection.QueryAsync<Ticket>(@"SELECT * FROM juniro.ticket 
                                                                        where TicketIssuerUserId = @userId", new { userId });
                if (queryResult == null)
                   return result;
                result = queryResult.AsList();
            }

            return result;
        }

        public async Task<List<CommentReport>> GetUserCommentReport(string userId)
        {
            var result = new List<CommentReport>();
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                var queryResult = await connection.QueryAsync<CommentReport>(@"SELECT * FROM juniro.commentreports 
                                                                        where ReportedByUserId = @userId", new { userId });
                if (queryResult == null)
                    return result;
                result = queryResult.AsList();
            }

            return result;
        }

        public async Task<List<PostReport>> GetUserPostReport(string userId)
        {
            var result = new List<PostReport>();
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                var queryResult = await connection.QueryAsync<PostReport>(@"SELECT * FROM juniro.postreports 
                                                                        where ReportedByUserId = @userId", new { userId });
                if (queryResult == null)
                    return result;
                result = queryResult.AsList();
            }

            return result;
        }
    }
}
