using System;
using System.Web.Mvc;

namespace WarframeUpdate.Filters
{
    /// <summary>
    /// Custom authorization attribute for Admin and SuperAdmin roles
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = "Admin,SuperAdmin";
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            if (!filterContext.HttpContext.User.IsInRole("Admin") && 
                !filterContext.HttpContext.User.IsInRole("SuperAdmin"))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}