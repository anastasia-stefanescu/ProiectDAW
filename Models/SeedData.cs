using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proiect.Data.Migrations;
using Proiect.Data;
using System;
using System.Collections.Generic;

namespace Proiect.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider
        serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return; 
                }
                

                // roluri pentru utilizator inregistrat si administrator
                // vizitator neinregistrat nu are rol
                context.Roles.AddRange(
                new IdentityRole
                {
                    Id = "e440db26-c31d-49d0-a975-830dec48b660", 
                    Name = "Admin", 
                    NormalizedName = "Admin".ToUpper() 
                },            
                new IdentityRole
                {
                    Id = "e440db26-c31d-49d0-a975-830dec48b661",
                    Name = "User", 
                    NormalizedName = "User".ToUpper()
                });
              

                var hasher = new PasswordHasher<ApplicationUser>();
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "b6ad85a7-7072-483b-ac09-90b56d39ff40",
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },
                new ApplicationUser
                {

                    Id = "b6ad85a7-7072-483b-ac09-90b56d39ff41",
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                }
                );
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "e440db26-c31d-49d0-a975-830dec48b660",
                    UserId = "b6ad85a7-7072-483b-ac09-90b56d39ff40"
                },
                new IdentityUserRole<string>

                {
                    RoleId = "e440db26-c31d-49d0-a975-830dec48b661",
                    UserId = "b6ad85a7-7072-483b-ac09-90b56d39ff41"
                });

                context.SaveChanges();
            }
        }
    }
}
