using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.IMySqlRepos;
using DataModelLayer.Models.Posts;
using DataModelLayer.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/ReportPost")]
    [Authorize]
    public class ReportPostController : Controller
    {
        private readonly IReportPostRepository _reportPostRepository;
        private readonly IAuthorizationHelper _authorizationHelper;
        private readonly ClaimsPrincipal _caller;

        public ReportPostController(
            IReportPostRepository reportPostRepository, 
            IAuthorizationHelper authorizationHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _reportPostRepository = reportPostRepository;
            _authorizationHelper = authorizationHelper;
            _caller = httpContextAccessor.HttpContext.User;
        }

        [HttpPost]
        [Route("ReportPost")]
        public async Task<IActionResult> Post([FromBody] ReportPostViewModel model)
        {
            var report = await MapViewModelToReport(model);
            
            if(await _authorizationHelper.CheckIfUserIsBanned(report.ReportById))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            report.Id = await _reportPostRepository.ReportPost(report);
            if (report.Id == 0)
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "Cannot report same post twice, edit your report.",
                        StatusCodes.Status409Conflict
                    });

            return new OkObjectResult(
                new
                {
                    Message = "Report created succesfully",
                    report
                });
        }

        private async Task<PostReport> MapViewModelToReport(ReportPostViewModel model)
        {
            return new PostReport
            {
                PostId = model.PostId,
                Reason = model.Reason,
                ReportTime = DateTime.Now,
                ReportById = await _authorizationHelper.GetCallerId(_caller)
            };
        }
    }
}