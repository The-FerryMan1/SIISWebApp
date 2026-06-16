using Microsoft.AspNetCore.Identity;

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
            string password = "Admin123!";


            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }


            var defaultAdmin = new IdentityUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(defaultAdmin, password);
            if (result.Succeeded) 
            {
                await userManager.AddToRoleAsync(defaultAdmin, role);
            }
        }
    }
}
