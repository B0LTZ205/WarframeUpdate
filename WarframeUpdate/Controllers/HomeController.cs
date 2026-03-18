using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WarframeTracker.Services;

namespace WarframeUpdate.Controllers
{
    public class HomeController : Controller
    {
        private readonly WarframeService _warframe = new WarframeService();

        public async Task<ActionResult> Index()
        {
            var vm = await _warframe.GetDashboardAsync();
            return View(vm);
        }
    }
}