using System.Collections.Generic;
using DataModelLayer.Models.Tikets;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.IUploadHelpers
{
    public interface ITicketUploadHelper
    {
        List<TicketFile> UploadFiles(List<IFormFile> ticketFiles, Ticket ticket);
    }
}
