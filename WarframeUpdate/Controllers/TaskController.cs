using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WarframeUpdate.Models;

namespace WarframeUpdate.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Task
        public ActionResult Index(TaskStatus? status = null)
        {
            var userId = User.Identity.GetUserId();
            var query = db.UserTasks.Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tasks = query.OrderByDescending(t => t.CreatedAt).ToList();
            return View(tasks);
        }

        // GET: Task/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var task = db.UserTasks
                .Include(t => t.Completions)
                .Include(t => t.Completions.Select(c => c.User))
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            if (task.UserId != userId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(task);
        }

        // GET: Task/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,Priority,DueDate")] UserTask task)
        {
            if (ModelState.IsValid)
            {
                task.UserId = User.Identity.GetUserId();
                task.CreatedAt = DateTime.UtcNow;
                task.Status = TaskStatus.Pending;

                db.UserTasks.Add(task);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(task);
        }

        // GET: Task/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var task = db.UserTasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            if (task.UserId != userId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(task);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Status,Priority,DueDate")] UserTask task)
        {
            if (ModelState.IsValid)
            {
                var existingTask = db.UserTasks.Find(task.Id);
                if (existingTask == null)
                {
                    return HttpNotFound();
                }

                var userId = User.Identity.GetUserId();
                if (existingTask.UserId != userId)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Status = task.Status;
                existingTask.Priority = task.Priority;
                existingTask.DueDate = task.DueDate;

                if (task.Status == TaskStatus.Completed && existingTask.Status != TaskStatus.Completed)
                {
                    existingTask.CompletedAt = DateTime.UtcNow;

                    var completion = new TaskCompletion
                    {
                        UserTaskId = task.Id,
                        CompletedBy = userId,
                        CompletedAt = DateTime.UtcNow
                    };

                    db.TaskCompletions.Add(completion);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(task);
        }

        // POST: Task/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var task = db.UserTasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            if (task.UserId != userId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            db.UserTasks.Remove(task);
            db.SaveChanges();

            return RedirectToAction("Index");
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