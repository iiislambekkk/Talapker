using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Talapker.Application.UserAccess.Queries.GetUserInfo;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Wolverine;

namespace Talapker.Web.Controllers.UserAccess;

public class AuthorizationController(IMessageBus messageBus) : Controller
{
    [HttpGet("/api/auth/verify")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public IActionResult Verify() => Ok();
    
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync("Identity.Application");

        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: "Identity.Application",
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var userId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);
    
        var claims = new List<Claim>
        {
            new Claim(OpenIddictConstants.Claims.Subject, userId),
            new Claim("stamp", user!.SecurityStamp!)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken),
            new Claim("firstName", result.Principal.FindFirstValue("FirstName") ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("lastName", result.Principal.FindFirstValue("LastName") ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("email", result.Principal.FindFirstValue("email") ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("image", result.Principal.FindFirstValue("image") ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("tenantId", result.Principal.FindFirstValue("tenantId") ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken),
        };

        var roleClaims = result.Principal.FindAll("roles");
        foreach (var roleClaim in roleClaims)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, roleClaim.Value)
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.AccessToken));
        }

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsPrincipal.SetScopes(request.GetScopes());

        var scopeManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>();
        var audiences = new HashSet<string>();
        foreach (var scope in request.GetScopes())
        {
            var scopeDescriptor = await scopeManager.FindByNameAsync(scope);
            if (scopeDescriptor is not null)
            {
                var resources = await scopeManager.GetResourcesAsync(scopeDescriptor);
                audiences.UnionWith(resources);
            }
        }

        claimsPrincipal.SetAudiences(audiences);
        
        Console.WriteLine("[AUTH] Claims being issued:");
        foreach (var claim in claims)
        {
            Console.WriteLine($"  {claim.Type} = {claim.Value} -> destinations: {string.Join(",", claim.GetDestinations())}");
        }


        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal? claimsPrincipal;

        if (request.IsAuthorizationCodeGrantType())
        {
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            claimsPrincipal = new ClaimsPrincipal(identity);

            identity.AddClaim(new Claim("client_id", request.ClientId ?? string.Empty));
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? string.Empty);

            claimsPrincipal.SetScopes(request.GetScopes());

            var scopeManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>();
            var audiences = new HashSet<string>();
            foreach (var scope in request.GetScopes())
            {
                var scopeDescriptor = await scopeManager.FindByNameAsync(scope);
                if (scopeDescriptor is not null)
                {
                    var resources = await scopeManager.GetResourcesAsync(scopeDescriptor);
                    audiences.UnionWith(resources);
                }
            }

            claimsPrincipal.SetAudiences(audiences);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            var userId = principal?.GetClaim(OpenIddictConstants.Claims.Subject);
            if (userId is null) return BadRequest();

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return BadRequest();

            var roles = await userManager.GetRolesAsync(user);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, userId)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            identity.AddClaim(new Claim("stamp", user.SecurityStamp!)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            identity.AddClaim(new Claim("tenantId", user.TenantId?.ToString() ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            identity.AddClaim(new Claim("firstName", user.FirstName ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken));
            identity.AddClaim(new Claim("lastName", user.LastName ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken));
            identity.AddClaim(new Claim("email", user.Email ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken));
            identity.AddClaim(new Claim("image", user.AvatarKey ?? "")
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken));

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role)
                    .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
            }

            claimsPrincipal = new ClaimsPrincipal(identity);
            claimsPrincipal.SetScopes(principal!.GetScopes());
        }
        else
        {
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        if (claimsPrincipal == null) claimsPrincipal = new ClaimsPrincipal();

        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/endsession")]
    [HttpPost("~/connect/endsession")]
    [IgnoreAntiforgeryToken]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync("Identity.Application");
        await HttpContext.SignOutAsync("OpenIddict.Server.AspNetCore");
        
    }

    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        var userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userId is null) return BadRequest();

        var res = await messageBus.InvokeAsync<UserInfoDto>(new GetUserInfoRequest(userId));

        return Ok(res);
    }
}