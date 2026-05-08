using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WarframeUpdate.Models
{
    // You may add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<EventSubscription> EventSubscriptions { get; set; }
        public virtual ICollection<FileUpload> FileUploads { get; set; }
        public virtual ICollection<AdminActivityLog> AdminActivityLogs { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual ICollection<TaskCompletion> TaskCompletions { get; set; }
        public virtual ICollection<NightwaveCompletion> NightwaveCompletions { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        public virtual UserProfile UserProfile { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }


        public DbSet<EventSubscription> EventSubscriptions { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<TaskCompletion> TaskCompletions { get; set; }
        public DbSet<NightwaveCompletion> NightwaveCompletions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserProfile to ApplicationUser relationship (one-to-one)
            // UserId is the primary key and foreign key
            modelBuilder.Entity<UserProfile>()
                .HasRequired(up => up.User)
                .WithOptional(au => au.UserProfile);

            // Configure UserTask relationships
            modelBuilder.Entity<UserTask>()
                .HasRequired(t => t.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserTask>()
                .HasMany(t => t.Completions)
                .WithRequired(tc => tc.UserTask)
                .HasForeignKey(tc => tc.UserTaskId)
                .WillCascadeOnDelete(true);

            // Configure TaskCompletion relationships
            modelBuilder.Entity<TaskCompletion>()
                .HasRequired(tc => tc.User)
                .WithMany(u => u.TaskCompletions)
                .HasForeignKey(tc => tc.CompletedBy)
                .WillCascadeOnDelete(false);

            // Configure NightwaveCompletion relationships
            modelBuilder.Entity<NightwaveCompletion>()
                .HasRequired(nc => nc.User)
                .WithMany(u => u.NightwaveCompletions)
                .HasForeignKey(nc => nc.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(true);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    // Event subscription model
    public class EventSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }

        public bool IsSubscribed { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; }
    }

    // Event types enum
    public enum EventTypes
    {
        Sortie,
        Nightwave,
        VoidFissures,
        BaroKiTeer,
        DailyDeals,
        Invasions,
        CetusCycle,
        VallisCycle
    }
}