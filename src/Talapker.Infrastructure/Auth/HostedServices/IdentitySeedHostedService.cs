using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.Auth.HostedServices;

public class IdentitySeedHostedService
(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<IdentitySeedHostedService> logger
) 
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting IdentitySeedHostedService...");

        using var scope = serviceScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var adminEmail = "islambekgazizovv@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            logger.LogInformation("Admin user with email {Email} already exists.", adminEmail);
            await EnsureRolesCreated(roleManager);
        }
        else
        {
            logger.LogInformation("Creating admin user with email {Email}...", adminEmail);

            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                FirstName = "Исламбек",
                LastName = "Газизов"
            };

            // Consider generating a secure temporary password or using a token flow
            var isSuccess = await userManager.CreateAsync(adminUser, adminEmail);

            if (!isSuccess.Succeeded)
            {
                foreach (var error in isSuccess.Errors)
                {
                    logger.LogError("Failed to create admin user. Code: {Code}, Description: {Description}", error.Code, error.Description);
                }

                return; // Exit if user creation failed
            }

            logger.LogInformation("Admin user {Email} created successfully.", adminEmail);
            
            
            await EnsureRolesCreated(roleManager);

            if (!await userManager.IsInRoleAsync(adminUser, UserRoles.SystemAdmin))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, UserRoles.SystemAdmin);
                if (addRoleResult.Succeeded)
                {
                    logger.LogInformation("Assigned SystemAdmin role to {Email}.", adminEmail);
                }
                else
                {
                    foreach (var error in addRoleResult.Errors)
                    {
                        logger.LogError("Failed to assign SystemAdmin role to {Email}. Code: {Code}, Description: {Description}", adminEmail, error.Code, error.Description);
                    }
                }
            }
            else
            {
                logger.LogInformation("User {Email} is already in SystemAdmin role.", adminEmail);
            }
        }

        logger.LogInformation("IdentitySeedHostedService finished.");
    }

    private async Task EnsureRolesCreated(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in UserRoles.AsList())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = role });
                if (result.Succeeded)
                {
                    logger.LogInformation("Role {Role} created successfully.", role);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("Failed to create role {Role}. Code: {Code}, Description: {Description}", role, error.Code, error.Description);
                    }
                }
            }
            else
            {
                logger.LogInformation("Role {Role} already exists.", role);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping IdentitySeedHostedService...");
        return Task.CompletedTask;
    }
}
