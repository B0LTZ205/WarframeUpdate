using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WarframeTracker.Services;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    public class HomeController : Controller
    {
        private readonly WarframeService _warframe = new WarframeService();

        public async Task<ActionResult> Index()
        {
            var vm = await _warframe.GetDashboardAsync();

            await CreateNightwaveReminderIfNeeded();

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

        private async Task CreateNightwaveReminderIfNeeded()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return;
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ApplicationDbContext())
            {
                bool subscribed = await db.EventSubscriptions
                    .AnyAsync(s => s.UserId == userId
                                && s.EventType == "Nightwave"
                                && s.IsSubscribed);

                if (!subscribed)
                {
                    return;
                }

                var today = DateTime.UtcNow.Date;

                bool alreadyCreated = await db.Notifications.AnyAsync(n =>
                    n.UserId == userId &&
                    n.Type == "Nightwave" &&
                    DbFunctions.TruncateTime(n.CreatedAt) == today);

                if (alreadyCreated)
                {
                    return;
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Nightwave Reminder",
                    Message = "Check your Nightwave challenges and complete them before the weekly reset.",
                    Type = "Nightwave",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                db.Notifications.Add(notification);
                await db.SaveChangesAsync();
            }
        }
    }
}