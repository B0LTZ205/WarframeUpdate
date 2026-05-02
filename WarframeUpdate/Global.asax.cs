using System;
using System.Web.Mvc;
using System.Web.Routing;
using WarframeUpdate.App_Start;

namespace WarframeUpdate
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Initialize roles
            RoleInitializer.InitializeRoles();
        }
    }
}
