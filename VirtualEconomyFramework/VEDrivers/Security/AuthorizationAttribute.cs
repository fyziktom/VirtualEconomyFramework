using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VEDrivers.Security
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(Rights rights) : base(typeof(AuthorizeActionFilter))
        {
            Arguments = new object[] { rights };
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly Rights rights;
        public AuthorizeActionFilter(Rights rights)
        {
            this.rights = rights;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;
            if (identity != null && identity.IsAuthenticated)
            {
                if (this.rights == 0) return; //no rights required, every logged user passes
                Enum.TryParse<Rights>((identity as ClaimsIdentity)?.Claims?.FirstOrDefault(c => c.Type == nameof(LoggedUser.Rights))?.Value, out var rights);
                if ((this.rights & rights) != 0) return; //user has at least one required right,  will pass
            }
            context.Result = new StatusCodeResult(403); //forbidden
        }
    }
}
