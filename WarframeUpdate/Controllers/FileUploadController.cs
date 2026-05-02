using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [Authorize]
    public class FileUploadController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Configuration constants
        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".flv" };
        private static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".flac", ".aac", ".ogg" };
        private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx" };

        // GET: FileUpload/Index
        [HttpGet]
        public ActionResult Index(int page = 1)
        {
            var userId = User.Identity.GetUserId();
            const int pageSize = 10;

            var files = db.FileUploads
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = db.FileUploads.Count(f => f.UserId == userId);

            var paginatedList = new PaginatedList<FileUpload>
            {
                Items = files,
                TotalCount = totalCount,
                PageIndex = page,
                PageSize = pageSize
            };

            return View(paginatedList);
        }

        // GET: FileUpload/Upload
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        // POST: FileUpload/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(string description = "")
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    ViewBag.ErrorMessage = "No file selected.";
                    return View();
                }

                var file = Request.Files[0];

                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    ViewBag.ErrorMessage = validationResult.ErrorMessage;
                    return View();
                }

                // Save file
                var userId = User.Identity.GetUserId();
                var fileUpload = SaveFile(file, userId, description);

                ViewBag.SuccessMessage = "File uploaded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View();
            }
        }

        // GET: FileUpload/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var fileUpload = db.FileUploads.Find(id);
            if (fileUpload == null || fileUpload.UserId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            return View(fileUpload);
        }

        // POST: FileUpload/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var fileUpload = db.FileUploads.Find(id);
            if (fileUpload == null || fileUpload.UserId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            try
            {
                // Delete physical file
                var filePath = Path.Combine(Server.MapPath("~/App_Data/Uploads"), fileUpload.StoredFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Delete database record
                db.FileUploads.Remove(fileUpload);
                db.SaveChanges();

                TempData["SuccessMessage"] = "File deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while deleting: {ex.Message}";
                return View(fileUpload);
            }
        }

        /// <summary>
        /// Validates file before upload
        /// </summary>
        private FileValidationResult ValidateFile(HttpPostedFileBase file)
        {
            var result = new FileValidationResult { IsValid = true };

            if (file == null || file.ContentLength == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "File is empty.";
                return result;
            }

            if (file.ContentLength > MaxFileSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.";
                return result;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!IsAllowedExtension(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "File type is not allowed. Allowed types: Images, Videos, Audio, Documents.";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Checks if file extension is allowed
        /// </summary>
        private bool IsAllowedExtension(string extension)
        {
            return AllowedImageExtensions.Contains(extension) ||
                   AllowedVideoExtensions.Contains(extension) ||
                   AllowedAudioExtensions.Contains(extension) ||
                   AllowedDocumentExtensions.Contains(extension);
        }

        /// <summary>
        /// Determines file type based on extension
        /// </summary>
        private string GetFileType(string extension)
        {
            if (AllowedImageExtensions.Contains(extension)) return "Image";
            if (AllowedVideoExtensions.Contains(extension)) return "Video";
            if (AllowedAudioExtensions.Contains(extension)) return "Audio";
            if (AllowedDocumentExtensions.Contains(extension)) return "Document";
            return "Other";
        }

        /// <summary>
        /// Saves file to disk and database
        /// </summary>
        private FileUpload SaveFile(HttpPostedFileBase file, string userId, string description)
        {
            var uploadsDir = Server.MapPath("~/App_Data/Uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, storedFileName);

            file.SaveAs(filePath);

            var fileUpload = new FileUpload
            {
                UserId = userId,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                FileExtension = extension,
                FileType = GetFileType(extension),
                FileSizeBytes = file.ContentLength,
                FilePath = $"/App_Data/Uploads/{storedFileName}",
                Description = description,
                UploadedAt = DateTime.UtcNow
            };

            db.FileUploads.Add(fileUpload);
            db.SaveChanges();

            return fileUpload;
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
    /// Helper model for file validation results
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}