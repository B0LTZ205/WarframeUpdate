namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using WarframeUpdate.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WarframeUpdate.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "WarframeUpdate.Models.ApplicationDbContext";
        }

        protected override void Seed(WarframeUpdate.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            // Create roles
            CreateRolesIfNotExist(context);

            // Create default admin user (optional)
            CreateDefaultAdminUser(context);
        }

        private void CreateRolesIfNotExist(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roleNames = { "SuperAdmin", "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
                }
            }

            context.SaveChanges();
        }

        private void CreateDefaultAdminUser(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Check if default admin exists
            if (userManager.FindByName("admin@warframe.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@warframe.com",
                    Email = "admin@warframe.com",
                    EmailConfirmed = true
                };

                var result = userManager.Create(user, "Admin@123");

                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "SuperAdmin");

                    // Create user profile
                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = "Super",
                        LastName = "Admin",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.UserProfiles.Add(profile);
                    context.SaveChanges();
                }
            }
        }
    }
}
