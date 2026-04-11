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

        // API endpoint to refresh dashboard data without page reload
        [HttpGet]
        public async Task<JsonResult> RefreshData()
        {
            try
            {
                var vm = await _warframe.GetDashboardAsync();
                return Json(new { success = true, data = vm }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HomeController] RefreshData error: {ex.Message}");
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}