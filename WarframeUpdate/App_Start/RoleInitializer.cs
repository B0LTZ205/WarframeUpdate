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
    }
}