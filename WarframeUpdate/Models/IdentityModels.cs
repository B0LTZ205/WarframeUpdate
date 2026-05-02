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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserProfile to ApplicationUser relationship (one-to-one)
            // UserId is the primary key and foreign key
            modelBuilder.Entity<UserProfile>()
                .HasRequired(up => up.User)
                .WithOptional(au => au.UserProfile);

            // Configure EventSubscription to ApplicationUser relationship (one-to-many)
            modelBuilder.Entity<EventSubscription>()
                .HasRequired(es => es.User)
                .WithMany(au => au.EventSubscriptions)
                .HasForeignKey(es => es.UserId)
                .WillCascadeOnDelete(true);

            // Configure FileUpload to ApplicationUser relationship (one-to-many)
            modelBuilder.Entity<FileUpload>()
                .HasRequired(fu => fu.User)
                .WithMany(au => au.FileUploads)
                .HasForeignKey(fu => fu.UserId)
                .WillCascadeOnDelete(true);

            // Configure AdminActivityLog to ApplicationUser relationship (one-to-many)
            modelBuilder.Entity<AdminActivityLog>()
                .HasRequired(aal => aal.AdminUser)
                .WithMany(au => au.AdminActivityLogs)
                .HasForeignKey(aal => aal.AdminUserId)
                .WillCascadeOnDelete(false);
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