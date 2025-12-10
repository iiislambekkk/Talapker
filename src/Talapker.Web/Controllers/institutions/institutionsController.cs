using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Talapker.Application;
using Talapker.Application.InstitutionFeatures.Commands.AssignPrimaryTenantAdmin;
using Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionAdvantages;
using Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionDescription;
using Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionGeneralInfo;
using Talapker.Application.InstitutionFeatures.Commands.CreateInstitution;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.Queries.GetAllInstitutions;
using Talapker.Application.InstitutionFeatures.Queries.GetById;
using Talapker.Application.InstitutionFeatures.Queries.GetByIdForAdmin;
using Talapker.Application.InstitutionFeatures.Queries.GetPrimaryAdmin;
using Talapker.Application.UserAccess.Queries;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;
using Talapker.Infrastructure.Data.UserAccess;
using Wolverine;

namespace Talapker.Web.Controllers.institutions;

[ApiController]
[Route("institutions")]
public class InstitutionsController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<List<InstitutionShortDto>>> GetInstitutions()
    {
        var institutions = await messageBus.InvokeAsync<List<InstitutionShortDto>>(new GetAllInstitutionsQuery());

        return Ok(institutions);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitutionDto>> GetInstitutions(Guid id)
    {
        var institution = await messageBus.InvokeAsync<InstitutionDto?>(new GetInstitutionByIdQuery(id));

        if (institution == null) return NotFound();
        
        return Ok(institution);
    }
    
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<InstitutionAdminDto>> GetInstitutionsForAdmin(Guid id)
    {
        var institution = await messageBus.InvokeAsync<InstitutionAdminDto?>(new GetInstitutionByIdForAdminQuery(id));

        if (institution == null) return NotFound();
        
        return Ok(institution);
    }
    
    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicies.SystemAdminOnly)]
    public async Task<ActionResult<ApiResponse>> Create(CreateInstitutionCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);

        return result.ToActionResult();
    }
    
    [HttpPut("{tenantId:guid}/edit/general-info")]
    [TenantAuthorize(IsAccessibleToSystemAdmin = true, Roles = $"{UserRoles.PrimaryTenantAdmin},{UserRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse>> ChangeGeneralInfo(ChangeInstitutionGeneralInfoCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);
        
        return result.ToActionResult();
    }
        
    [HttpPut("{tenantId:guid}/edit/description")]
    [TenantAuthorize(Policy = AuthorizationPolicies.TenantAdmins)]
    public async Task<ActionResult<ApiResponse>> ChangeDescription(ChangeInstitutionDescriptionCommand command)
    {
        await messageBus.InvokeAsync(command);
        return Ok();
    }
    
    [HttpPut("{tenantId:guid}/edit/advantages")]
    [TenantAuthorize(Policy = AuthorizationPolicies.TenantAdmins)]
    public async Task<ActionResult<ApiResponse>> ChangeDescription(ChangeInstitutionAdvantagesCommand command)
    {
        await messageBus.InvokeAsync(command);
        return Ok();
    }
    
    [HttpGet("{institutionId:guid}/primary-admin")]
    [Authorize(Policy = AuthorizationPolicies.SystemAdminOnly)]
    public async Task<ActionResult<UserDto?>> GetPrimaryAdmin(Guid institutionId)
    {
        var admin = await messageBus.InvokeAsync<UserDto?>(new GetPrimaryAdminQuery(institutionId));
        
        return Ok(admin);
    }
    
    [HttpPost("assign/primary-admin")]
    [Authorize(AuthenticationSchemes="OpenIddict.Validation.AspNetCore", Roles = UserRoles.SystemAdmin)]
    public async Task<ActionResult<ApiResponse>> AssignPrimaryAdmin(AssignPrimaryTenantAdminCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResponse>(command);

        return result.ToActionResult();
    }
}