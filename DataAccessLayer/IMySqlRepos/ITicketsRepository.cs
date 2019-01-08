using DataModelLayer.Models.Tikets;
using System.Threading.Tasks;

namespace DataAccessLayer.IMySqlRepos
{
    public interface ITicketsRepository
    {
        Task<int> CreateTicket(Ticket ticket, string callerId);
        Task<int> ReportEntity(ReportEntity reportEntity);
        Task<Ticket> AddFilesToTicket(Ticket ticket);
        Task<ReportEntity> AddFilesToReportEntity(ReportEntity reportEntity);
    }
}
