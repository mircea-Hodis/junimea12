using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Tikets;
using DataModelLayer.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/AddressingTicket")]
    [Authorize(Policy = "ApiAdmin")]
    // ReSharper disable once HollowTypeName
    public class TicketsAdminController : Controller
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ClaimsPrincipal _caller;

        public TicketsAdminController(
            ITicketsRepository ticketsRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _ticketsRepository = ticketsRepository;
            _caller = httpContextAccessor.HttpContext.User;
        }

        [Route("AddressTicket")]
        [HttpPost]
        public async Task<IActionResult> AddressTicket([FromBody]AddressTicketViewModel ticketViewModel)
        {
            var callerId = GetCallerId();
            if (string.IsNullOrEmpty(callerId))
                return new BadRequestObjectResult(new
                {
                    Message = "You must be logged in to address ticket!"
                });

            var result = await _ticketsRepository.AddressTicket(ticketViewModel, DateTime.Now, callerId);
            return new OkObjectResult(new
            {
                Message = "Ticket addressed!",
                result,
                DateTime.Now,
                callerId
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

        private PostReport MapTicketViewModelIntoDataObject(PostReportViewModel viewModel, string callerId)
        {
            return new PostReport
            {
                ReportedByUserId = callerId,
                EntityId = viewModel.EntityId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now,
            };
        }

        private CommentReport MapTicketViewModelIntoDataObject(CommentReportViewModel viewModel, string callerId)
        {
            return new CommentReport
            {
                ReportedByUserId = callerId,
                PostId = viewModel.PostId,
                EntityId = viewModel.EntityId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now
            };
        }

        [Route("UnbanUser")]
        [HttpPost]
        public async Task<IActionResult> UnBanUser([FromBody]UnbanUserViewModel model)
        {
            var callerId = GetCallerId();
            if (string.IsNullOrEmpty(callerId))
                return new BadRequestObjectResult(new
                {
                    Message = "Login to unban"
                });
            return Ok();
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