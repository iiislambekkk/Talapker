using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.File.DeleteFile;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;
using Talapker.Infrastructure.Exceptions;
using Wolverine;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionGeneralInfo;

public class ChangeInstitutionGeneralInfoCommandHandler
{
    public async Task<ApiResponse> Handle(ChangeInstitutionGeneralInfoCommand command, TalapkerDbContext db, IMessageBus messageBus)
    {
        var institution = await db.Institutions.FirstOrDefaultAsync(i => i.Id == command.Id);
        
        if (institution == null) throw new BadRequestException("Institution not found");
        
        if (command.Type == InstitutionType.University)
        {
            var institutionWithSameCode =
                await db.Institutions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.NationalCode == command.NationalCode && i.Id != command.Id);
            
            if (institutionWithSameCode != null)
            {
                return ApiResponse.Fail
                (
                    "University with same national code already exists",  
                    ErrorCodes.UniversityWithSameNationalCodeAlreadyExist, 
                    StatusCodes.Status409Conflict
                );   
            }
        }

        if (institution.LogoKey != command.FileKey)
        {
            await messageBus.PublishAsync(new DeleteFileCommand(institution.LogoKey));
        }

        institution.NationalCode = command.NationalCode;
        institution.Name = command.Name;
        institution.LogoKey = command.FileKey;
        institution.Type = command.Type;
        
        db.Institutions.Update(institution);
        await db.SaveChangesAsync();
        
        return ApiResponse.Success();
    }
}