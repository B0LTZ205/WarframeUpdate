using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [Authorize]
    public class PreferencesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Preferences
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var subscriptions = await db.EventSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            // Ensure all event types exist for this user
            foreach (EventTypes eventType in Enum.GetValues(typeof(EventTypes)))
            {
                if (!subscriptions.Any(s => s.EventType == eventType.ToString()))
                {
                    var subscription = new EventSubscription
                    {
                        UserId = userId,
                        EventType = eventType.ToString(),
                        IsSubscribed = true
                    };
                    db.EventSubscriptions.Add(subscription);
                }
            }

            await db.SaveChangesAsync();

            subscriptions = await db.EventSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(subscriptions);
        }

        // POST: Toggle subscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleSubscription(string eventType)
        {
            var userId = User.Identity.GetUserId();
            var subscription = await db.EventSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EventType == eventType);

            if (subscription == null)
            {
                subscription = new EventSubscription
                {
                    UserId = userId,
                    EventType = eventType,
                    IsSubscribed = true
                };
                db.EventSubscriptions.Add(subscription);
            }
            else
            {
                subscription.IsSubscribed = !subscription.IsSubscribed;
            }

            await db.SaveChangesAsync();

            return Json(new { success = true, isSubscribed = subscription.IsSubscribed });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}