using System;
using DataModelLayer.Models.Tikets;
using System.Threading.Tasks;

namespace DataAccessLayer.IMySqlRepos
{
    public interface ITicketsRepository
    {
        Task<Ticket> CreateTicket(Ticket ticket, string callerId);
        Task<int> ReportEntity(ReportEntity reportEntity);
        Task<bool> AddressTicket(AddressTicketViewModel ticketId, DateTime stagedDate, string stagedByUserId);
    }
}
