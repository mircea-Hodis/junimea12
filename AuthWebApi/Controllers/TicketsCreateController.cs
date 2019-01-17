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

        private ReportEntity MapTicketViewModelIntoDataObject(ReportEntityViewModel viewModel, string callerId)
        {
            return new ReportEntity
            {
                ReportedByUserId = callerId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now,
                ReportedEntityId = viewModel.ReportedEntityId
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