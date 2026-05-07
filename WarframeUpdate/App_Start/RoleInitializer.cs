using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WarframeUpdate.Models;

namespace WarframeUpdate.App_Start
{
    /// <summary>
    /// Initializes default roles in the application
    /// </summary>
    public static class RoleInitializer
    {
        public static void InitializeRoles()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var roleManager = new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(context));

                    // Create roles if they don't exist
                    CreateRoleIfNotExists(roleManager, "SuperAdmin");
                    CreateRoleIfNotExists(roleManager, "Admin");
                    CreateRoleIfNotExists(roleManager, "User");

                    // Create default admin account
                    CreateDefaultAdminAccount(context);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing roles: {ex.Message}");
            }
        }

        private static void CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }
        }

        private static void CreateDefaultAdminAccount(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            const string adminEmail = "admin2@warframe.com";
            const string adminPassword = "Admin2@123456"; // Change this to a secure password

            // Check if admin account already exists
            if (userManager.FindByEmail(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var result = userManager.Create(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Assign SuperAdmin role
                    userManager.AddToRole(adminUser.Id, "SuperAdmin");
                    userManager.AddToRole(adminUser.Id, "Admin");
                    System.Diagnostics.Debug.WriteLine($"Admin account '{adminEmail}' created successfully.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating admin account: {string.Join(", ", result.Errors)}");
                }
            }
        }
    }
}