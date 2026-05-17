using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WarframeUpdate.Filters;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(db)
                );
            }
            private set
            {
                _roleManager = value;
            }
        }

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
        public ActionResult Users(int page = 1, string search = "", string sortBy = "username", string sortOrder = "asc")
        {
            const int pageSize = 10;

            var query = db.Users.Include("UserProfile").AsQueryable();

            // Search by username or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            // Sort
            switch ((sortBy ?? "username").ToLower())
            {
                case "email":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email);
                    break;
                case "emailstatus":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(u => u.EmailConfirmed)
                        : query.OrderBy(u => u.EmailConfirmed);
                    break;
                default:
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(u => u.UserName)
                        : query.OrderBy(u => u.UserName);
                    break;
            }

            var totalCount = query.Count();
            var users = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedList = new PaginatedList<ApplicationUser>
            {
                Items = users,
                TotalCount = totalCount,
                PageIndex = page,
                PageSize = pageSize
            };

            ViewBag.Search    = search ?? "";
            ViewBag.SortBy    = (sortBy ?? "username").ToLower();
            ViewBag.SortOrder = sortOrder ?? "asc";

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

            ViewBag.IsAdmin = UserManager.IsInRole(id, "Admin");

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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MakeAdmin(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Create Admin role if missing
            if (!await RoleManager.RoleExistsAsync("Admin"))
            {
                await RoleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Prevent duplicate role assignment
            if (!await UserManager.IsInRoleAsync(user.Id, "Admin"))
            {
                await UserManager.AddToRoleAsync(user.Id, "Admin");

                LogAdminActivity($"Promoted User To Admin: {user.UserName}");

                TempData["SuccessMessage"] = $"{user.UserName} is now an admin.";
            }

            return RedirectToAction("UserDetails", new { id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveAdmin(string id)
        {
            var currentUserId = User.Identity.GetUserId();

            // Prevent removing yourself
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot remove your own admin role.";
                return RedirectToAction("UserDetails", new { id });
            }

            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
            {
                await UserManager.RemoveFromRoleAsync(user.Id, "Admin");

                LogAdminActivity($"Removed Admin Role: {user.UserName}");

                TempData["SuccessMessage"] = $"{user.UserName} is no longer an admin.";
            }

            return RedirectToAction("UserDetails", new { id });
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmEmail(string id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.EmailConfirmed = true;
            db.SaveChanges();

            LogAdminActivity($"Force confirmed email for user: {user.UserName}");

            TempData["SuccessMessage"] = $"{user.UserName}'s email has been confirmed.";

            return RedirectToAction("UserDetails", new { id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnconfirmEmail(string id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.EmailConfirmed = false;
            db.SaveChanges();

            LogAdminActivity($"Removed email confirmation for user: {user.UserName}");

            TempData["SuccessMessage"] = $"{user.UserName}'s email confirmation has been removed.";

            return RedirectToAction("UserDetails", new { id });
        }

        // GET: Admin/EditUser/5
        public ActionResult EditUser(string id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminEditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed
            };

            return View(viewModel);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.Find(model.Id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;

            db.SaveChanges();

            LogAdminActivity($"Edited User Info: {user.UserName}");

            TempData["SuccessMessage"] = "User information updated successfully.";

            return RedirectToAction("UserDetails", new { id = user.Id });
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public ActionResult DeleteProfilePicture(string id)
{
    var profile = db.UserProfiles.FirstOrDefault(p => p.UserId == id);

    if (profile == null)
    {
        TempData["ErrorMessage"] = "User profile was not found.";
        return RedirectToAction("UserDetails", new { id });
    }

    profile.ProfilePictureUrl = null; // change this name if your column is different
    db.SaveChanges();

    LogAdminActivity($"Deleted profile picture for user ID: {id}");

    TempData["SuccessMessage"] = "Profile picture removed successfully.";

    return RedirectToAction("UserDetails", new { id });
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

    public class AdminEditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
    }
}