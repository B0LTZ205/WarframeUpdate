using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

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

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.UpdateProfileSuccess ? "Your profile has been updated."
                : "";

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var userProfile = db.UserProfiles.FirstOrDefault(p => p.UserId == userId);

            var viewModel = new ManageProfileViewModel
            {
                User = user,
                UserProfile = userProfile
            };

            return View(viewModel);
        }

        // GET: /Manage/UpdateProfile
        public ActionResult UpdateProfile()
        {
            var userId = User.Identity.GetUserId();
            var userProfile = db.UserProfiles.FirstOrDefault(p => p.UserId == userId);

            if (userProfile == null)
            {
                userProfile = new UserProfile { UserId = userId };
            }

            return View(userProfile);
        }

        // POST: /Manage/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UserProfile model)
        {
            var userId = User.Identity.GetUserId();
            var userProfile = db.UserProfiles.FirstOrDefault(p => p.UserId == userId);

            if (userProfile == null)
            {
                userProfile = new UserProfile { UserId = userId };
                db.UserProfiles.Add(userProfile);
            }

            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.UpdatedAt = DateTime.UtcNow;

            // Handle profile picture upload
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    var validationResult = ValidateProfilePicture(file);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("", validationResult.ErrorMessage);
                        return View(userProfile);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(userProfile.ProfilePictureUrl))
                    {
                        try
                        {
                            var oldFilePath = Server.MapPath(userProfile.ProfilePictureUrl);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        catch { }
                    }

                    // Save new profile picture
                    userProfile.ProfilePictureUrl = SaveProfilePicture(file, userId);
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index", new { Message = ManageMessageId.UpdateProfileSuccess });
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }
            return View(model);
        }

        // Helper functions
        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        private FileValidationResult ValidateProfilePicture(HttpPostedFileBase file)
        {
            var result = new FileValidationResult { IsValid = true };
            const long maxSize = 5 * 1024 * 1024; // 5 MB
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

            if (file.ContentLength > maxSize)
            {
                result.IsValid = false;
                result.ErrorMessage = "Profile picture must be less than 5 MB.";
                return result;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "Only image files (jpg, png, gif) are allowed.";
                return result;
            }

            return result;
        }

        private string SaveProfilePicture(HttpPostedFileBase file, string userId)
        {
            var uploadsDir = Server.MapPath("~/Content/ProfilePictures");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            file.SaveAs(filePath);

            return $"/Content/ProfilePictures/{fileName}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (db != null)
                {
                    db.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemovePhoneSuccess,
            UpdateProfileSuccess,
            Error
        }

        #endregion
    }
}
