using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuthWebApi.IUploadHelpers;
using DataModelLayer.Models.Tikets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.UploadHelpers
{
    public class TicketUploadHelper : ITicketUploadHelper
    {
        private readonly IHostingEnvironment _environment;

        public TicketUploadHelper(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public List<TicketFile> UploadFiles(List<IFormFile> ticketFiles, Ticket ticket)
        {
            foreach (var file in ticketFiles)
            {
                var uploadedPath = Path.Combine(_environment.ContentRootPath, "images");
             //   ticket.TicketFile.Add(new TicketFile());
            }

            return new List<TicketFile>();
        }
    }
}
