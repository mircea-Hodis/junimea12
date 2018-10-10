﻿using AuthWebApi.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace AuthWebApi.Authorize
{
    public class AuthorizationHelper : IAuthorizationHelper
    {
        private readonly ClaimsPrincipal _caller;
        private readonly MsSqlUserDbContext _appDbContext;

        public AuthorizationHelper(IHttpContextAccessor httpContextAccessor, MsSqlUserDbContext appDbContenxt)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContenxt;
        }

        public Claim GetCallerId()
        {
            return _caller.Claims.Single(c => c.Type == "id");
        }

    }

}
