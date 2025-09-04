//using LibraryManagementSystem.Models;
//using Microsoft.AspNetCore.Identity;

//public static class RoleSeeder
//{
//    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
//    {
//        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//        // Ensure roles exist
//        if (!await roleManager.RoleExistsAsync("Admin"))
//            await roleManager.CreateAsync(new IdentityRole("Admin"));

//        if (!await roleManager.RoleExistsAsync("User"))
//            await roleManager.CreateAsync(new IdentityRole("User"));

//        // Go through all users
//        var users = userManager.Users.ToList();
//        foreach (var user in users)
//        {
//            var roles = await userManager.GetRolesAsync(user);

//            // Skip if user already has "Admin"
//            if (roles.Contains("Admin"))
//                continue;

//            // Add "User" role if not already
//            if (!roles.Contains("User"))
//            {
//                await userManager.AddToRoleAsync(user, "User");
//            }
//        }
//    }
//}
