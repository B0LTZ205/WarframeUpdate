using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace WarframeUpdate
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Admin routes
            routes.MapRoute(
                name: "AdminDashboard",
                url: "admin",
                defaults: new { controller = "Admin", action = "Dashboard" }
            );

            // File upload routes
            routes.MapRoute(
                name: "FileUpload",
                url: "files/{action}/{id}",
                defaults: new { controller = "FileUpload", action = "Index", id = UrlParameter.Optional }
            );

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
