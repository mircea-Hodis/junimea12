using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Tikets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Ticket")]
    [Authorize]
    // ReSharper disable once HollowTypeName
    public class TicketsCreateController : Controller
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ClaimsPrincipal _caller;

        public TicketsCreateController(
            ITicketsRepository ticketsRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _ticketsRepository = ticketsRepository;
            _caller = httpContextAccessor.HttpContext.User;
        }

        [Route("AddTicket")]
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody]TicketsViewModel ticketViewModel)
        {
            var callerId = GetCallerId();
            if (string.IsNullOrEmpty(callerId))
                return new BadRequestObjectResult(new
                {
                    Message = "You must be logged in to report!"
                });
            var result = await _ticketsRepository.CreateTicket(
                MapTicketViewModelIntoDataObject(ticketViewModel, callerId), 
                callerId);

            return new OkObjectResult(new
            {
                Message = "Report added1",
                result
            });
        }

        private Ticket MapTicketViewModelIntoDataObject(TicketsViewModel viewModel, string callerId)
        {
            return new Ticket
            {
                TicketIssuerUserId = callerId,
                Message = viewModel.Message,
                IsPending = true,
                CreatedDate = DateTime.Now,
                AddressedMessage = string.Empty,
                AddressedById = string.Empty
            };
        }

        [Route("ReportPost")]
        [HttpPost]
        public async Task<IActionResult> ReportPost(PostReportViewModel viewModel)
        {
            var callerId = GetCallerId();
            var postReport = MapTicketViewModelIntoDataObject(viewModel, callerId);

            postReport.Id = await _ticketsRepository.ReportPost(postReport);

            return new OkObjectResult(new
            {
                Message = "Report added",
                postReport
            });
        }

        [Route("ReportComment")]
        [HttpPost]
        public  async Task<IActionResult> ReportComment(CommentReportViewModel viewModel)
        {
            var callerId = GetCallerId();
            var commentReport = MapReportCommentViewModel(viewModel, callerId);

            commentReport.Id = await _ticketsRepository.ReportComment(commentReport);

            return new OkObjectResult(new
            {
                Message = "Report added",
                postReport = commentReport
            });
        }

        [Route("GetTickets")]
        public async Task<IActionResult> GetTickets()
        {
            var callerId = GetCallerId();

            var result = await _ticketsRepository.GetUserTicket(callerId);

            return new OkObjectResult(new
            {
                Message = result.Count > 0 ? "Your current tickets" : "you have no tickets",
                postReport = result
            });
        }

        [Route("GetCommentReports")]
        public async Task<IActionResult> GetCommentReports()
        {
            var callerId = GetCallerId();

            var result = await _ticketsRepository.GetUserCommentReport(callerId);

            return new OkObjectResult(new
            {
                Message = result.Count > 0 ? "Your current reports" : "you have no reports",
                postReport = result
            });
        }

        [Route("GetUserPostReports")]
        public async Task<IActionResult> GetUserPostReports()
        {
            var callerId = GetCallerId();

            var result = await _ticketsRepository.GetUserPostReport(callerId);

            return new OkObjectResult(new
            {
                Message = result.Count > 0 ? "Your current reports" : "you have no reports",
                postReport = result
            });
        }

        private PostReport MapTicketViewModelIntoDataObject(PostReportViewModel viewModel, string callerId)
        {
            return new PostReport
            {
                ReportedByUserId = callerId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now,
            };
        }

        private CommentReport MapReportCommentViewModel(CommentReportViewModel viewModel, string callerId)
        {
            return new CommentReport
            {
                PostId = viewModel.PostId,
                ReportedByUserId = callerId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now,
            };
        }

        private string GetCallerId()
        {
            if (_caller.Identity.IsAuthenticated)
                return _caller.Claims.Single(claim =>
                        string.Equals(claim.Type, "id", StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4);
            return string.Empty;
        }
    }
}