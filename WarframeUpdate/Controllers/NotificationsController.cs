using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var notifications = await db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var userId = User.Identity.GetUserId();

            var notification = await db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = User.Identity.GetUserId();

            var notifications = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult UnreadCount()
        {
            var userId = User.Identity.GetUserId();

            int count = db.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);

            return PartialView("_UnreadCount", count);
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