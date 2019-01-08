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
        public async Task<IActionResult> AddTicket([FromForm]TicketsViewModel ticketViewModel)
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
            if (result > 0 && ticketViewModel.TicketFiles.Any())
            {
                
            }

            return new OkObjectResult(new
            {
                Message = "User banned succesfully",
                result
            });

        }

        private Ticket MapTicketViewModelIntoDataObject(TicketsViewModel viewModel, string callerId)
        {
            return new Ticket
            {
                TicketIssuerUserId = callerId,
                Message = viewModel.Message,
                CreatedDate = DateTime.Now
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