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
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = service.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureCreated();

            string role = "Admin";
            string email = "admin@gmail.com";
            string username = "admin";
            string password = "Admin123!";

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            var defaultAdmin = new IdentityUser
            {
                Email = email,
                UserName = username,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(defaultAdmin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultAdmin, role);
            }
        }

        public static async Task InitOffices(IServiceProvider service)
        {
            var dbContext = service.GetRequiredService<AppDbContext>();

            var officeCount = await dbContext.Offices.CountAsync();
            if (officeCount == 0)
            {
                foreach (OfficeNameEnum office in Enum.GetValues<OfficeNameEnum>())
                {
                    await dbContext.Offices.AddAsync(new Features.Shared.Models.OfficeModel
                    {
                        Name = office
                    });
                }
                await dbContext.SaveChangesAsync();
            }

            var offices = await dbContext.Offices.Where(o => !o.IsDeleted).ToListAsync();
            var hasher = new PasswordHasher<OfficeAccountModel>();

            foreach (var office in offices)
            {
                var existing = await dbContext.OfficeAccounts
                    .AnyAsync(a => a.OfficeId == office.Id && !a.IsDeleted);

                if (!existing)
                {
                    await dbContext.OfficeAccounts.AddAsync(new Features.Shared.Models.OfficeAccountModel
                    {
                        OfficeId = office.Id,
                        Username = OfficeEnumLabels.GetLabel(office.Name).ToLower().Replace(" ", ""),
                        Email = $"{office.Name.ToString().ToLower()}@siis.local",
                        PasswordHash = hasher.HashPassword(null, "Admin123!"),
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}