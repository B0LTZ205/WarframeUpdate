using System.Web;
using System.Web.Mvc;

namespace WarframeUpdate
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Require authentication site-wide (use AllowAnonymous on public actions)
            // filters.Add(new AuthorizeAttribute());
        }
    }
}
