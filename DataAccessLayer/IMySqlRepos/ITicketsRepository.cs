using System;
using System.Collections.Generic;
using DataModelLayer.Models.Tikets;
using System.Threading.Tasks;

namespace DataAccessLayer.IMySqlRepos
{
    public interface ITicketsRepository
    {
        Task<Ticket> CreateTicket(Ticket ticket, string callerId);
        Task<int> ReportPost(PostReport postReport);
        Task<int> AddressPostReport(PostReport postReport);
        Task<bool> AddressTicket(AddressTicketViewModel ticketId, DateTime stagedDate, string stagedByUserId);
        Task<int> ReportComment(CommentReport postReport);
        Task<int> AddressCommentReport(CommentReport postReport);
        Task<List<Ticket>> GetUserTicket(string userId);
        Task<List<CommentReport>> GetUserCommentReport(string userId);
        Task<List<PostReport>> GetUserPostReport(string userId);
    }
}
