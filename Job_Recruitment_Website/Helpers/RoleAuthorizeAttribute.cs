using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Helpers
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;
        public RoleAuthorizeAttribute(params string[] roles) { _roles = roles; }

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var role = ctx.HttpContext.Session["Role"] as string;
            if (role == null)
                ctx.Result = new RedirectResult("~/Account/Login");
            else if (_roles.Length > 0 && !_roles.Contains(role))
                ctx.Result = new HttpStatusCodeResult(403);
        }
    }
}