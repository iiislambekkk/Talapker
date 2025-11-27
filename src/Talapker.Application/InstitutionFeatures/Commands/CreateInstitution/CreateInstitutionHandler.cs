using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.Commands.CreateInstitution;

public class CreateInstitutionHandler
{
    public async Task<ApiResponse> Handle(CreateInstitutionCommand command, TalapkerDbContext db)
    {
        if (command.Type == InstitutionType.University)
        {
            var isThereInstitution =
                await db.Institutions.AsNoTracking().FirstOrDefaultAsync(i => i.NationalCode == command.NationalCode);
            
            if (isThereInstitution != null)
            {
                return ApiResponse.Fail
                    (
                        "University with same national code already exists",  
                        ErrorCodes.UniversityWithSameNationalCodeAlreadyExist, 
                        StatusCodes.Status409Conflict
                    ); 
            }
        }

        var institution = new Institution()
        {
            Name = command.Name,
            NationalCode = command.NationalCode, 
            Type = command.Type,
            LogoKey = command.FileKey
        };
        
        await db.Institutions.AddAsync(institution);
        await db.SaveChangesAsync();
        
        return ApiResponse.Success();
    }
}