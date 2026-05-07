using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using WarframeUpdate.Filters;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = db.Users.Count(),
                TotalFileUploads = db.FileUploads.Count(),
                TotalEventSubscriptions = db.EventSubscriptions.Count(),
                RecentActivityLogs = db.AdminActivityLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList()
            };

            LogAdminActivity("Accessed Dashboard");
            return View(viewModel);
        }

        // GET: Admin/Users
        public ActionResult Users(int page = 1)
        {
            const int pageSize = 10;
            var users = db.Users
                .Include("UserProfile") 
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = db.Users.Count();

            var paginatedList = new PaginatedList<ApplicationUser>
            {
                Items = users,
                TotalCount = totalCount,
                PageIndex = page,
                PageSize = pageSize
            };

            LogAdminActivity("Viewed Users List");
            return View(paginatedList);
        }

        // GET: Admin/UserDetails/5
        public ActionResult UserDetails(string id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userProfile = db.UserProfiles.FirstOrDefault(p => p.UserId == id);
            var fileUploads = db.FileUploads.Where(f => f.UserId == id).ToList();
            var subscriptions = db.EventSubscriptions.Where(s => s.UserId == id).ToList();

            var viewModel = new AdminUserDetailsViewModel
            {
                User = user,
                UserProfile = userProfile,
                FileUploads = fileUploads,
                EventSubscriptions = subscriptions
            };

            LogAdminActivity($"Viewed User Details: {user.UserName}");
            return View(viewModel);
        }

        // GET: Admin/ActivityLogs
        public ActionResult ActivityLogs(int page = 1)
        {
            const int pageSize = 20;
            var logs = db.AdminActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = db.AdminActivityLogs.Count();

            var paginatedList = new PaginatedList<AdminActivityLog>
            {
                Items = logs,
                TotalCount = totalCount,
                PageIndex = page,
                PageSize = pageSize
            };

            return View(paginatedList);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Delete related data
                var fileUploads = db.FileUploads.Where(f => f.UserId == id).ToList();
                db.FileUploads.RemoveRange(fileUploads);

                var subscriptions = db.EventSubscriptions.Where(s => s.UserId == id).ToList();
                db.EventSubscriptions.RemoveRange(subscriptions);

                var profile = db.UserProfiles.FirstOrDefault(p => p.UserId == id);
                if (profile != null)
                {
                    db.UserProfiles.Remove(profile);
                }

                db.Users.Remove(user);
                db.SaveChanges();

                LogAdminActivity($"Deleted User: {user.UserName}");
                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("UserDetails", new { id });
            }
        }

        /// <summary>
        /// Logs admin activity
        /// </summary>
        private void LogAdminActivity(string action, string details = "")
        {
            var adminUserId = User.Identity.GetUserId();
            var log = new AdminActivityLog
            {
                AdminUserId = adminUserId,
                Action = action,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            db.AdminActivityLogs.Add(log);
            db.SaveChanges();
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

    /// <summary>
    /// View model for admin dashboard
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalFileUploads { get; set; }
        public int TotalEventSubscriptions { get; set; }
        public List<AdminActivityLog> RecentActivityLogs { get; set; }
    }

    /// <summary>
    /// View model for user details
    /// </summary>
    public class AdminUserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public UserProfile UserProfile { get; set; }
        public List<FileUpload> FileUploads { get; set; }
        public List<EventSubscription> EventSubscriptions { get; set; }
    }
}