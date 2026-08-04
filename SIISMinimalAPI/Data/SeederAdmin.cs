using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Data
{
    public static class SeederAdmin
    {
        public static async Task InitAdmin(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<User>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = service.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureCreated();

            // Create roles
            string[] roles = { "Admin", "OPG", "Officer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            string adminEmail = "admin@gmail.com";
            string adminUsername = "admin";
            string adminPassword = "Admin123!";

            var defaultAdmin = new User
            {
                Email = adminEmail,
                UserName = adminUsername,
                EmailConfirmed = true,
                LastName = "Admin",
                FirstName = "System",
                MiddleName = ""
            };
            var result = await userManager.CreateAsync(defaultAdmin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultAdmin, "Admin");
            }

            // Create OPG user
            string opgEmail = "opg@gmail.com";
            string opgUsername = "opg";
            var opgUser = new User
            {
                Email = opgEmail,
                UserName = opgUsername,
                EmailConfirmed = true,
                LastName = "OPG",
                FirstName = "User",
                MiddleName = ""
            };
            var opgResult = await userManager.CreateAsync(opgUser, "Admin123!");
            if (opgResult.Succeeded)
            {
                await userManager.AddToRoleAsync(opgUser, "OPG");
            }

            // Create Officer users for each office
            var officeCount = await dbContext.Offices.CountAsync();
            if (officeCount == 0)
            {
                foreach (OfficeNameEnum office in Enum.GetValues<OfficeNameEnum>())
                {
                    await dbContext.Offices.AddAsync(new Office
                    {
                        OfficeName = OfficeEnumLabels.GetLabel(office),
                        UserId = null
                    });
                }
                await dbContext.SaveChangesAsync();
            }

            var offices = await dbContext.Offices.Where(o => !o.IsDeleted).ToListAsync();
            foreach (var office in offices)
            {
                string officerUsername = office.OfficeName.ToLower().Replace(" ", "");
                string officerEmail = $"{office.OfficeName.ToLower().Replace(" ", "")}@siis.local";
                string officerPassword = "Admin123!";

                var officer = new User
                {
                    Email = officerEmail,
                    UserName = officerUsername,
                    EmailConfirmed = true,
                    LastName = "Officer",
                    FirstName = office.OfficeName,
                    MiddleName = ""
                };

                var officerResult = await userManager.CreateAsync(officer, officerPassword);
                if (officerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(officer, "Officer");
                    office.UserId = officer.Id;
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}