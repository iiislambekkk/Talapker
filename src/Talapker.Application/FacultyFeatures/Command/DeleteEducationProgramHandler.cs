using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Talapker.Application.AI.Talapker;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.FacultyFeatures.Command;

public record DeleteEducationProgramCommand(
    Guid Id
);

public class DeleteEducationProgramHandler
{
    public async Task<ApiResponse> Handle(
        DeleteEducationProgramCommand command,
        TalapkerDbContext db,
        IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var program = await db.EducationPrograms
            .Include(p => p.Faculty)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (program == null)
            return ApiResponse.Fail("Education program not found", ErrorCodes.Default);
        
        if (program.Faculty != null) await TalapkerToolsCache.InvalidateInstitutionContextAsync(cache, program.Faculty.InstitutionId, cancellationToken);

        db.EducationPrograms.Remove(program);
        await db.SaveChangesAsync(cancellationToken);
        
        

        return ApiResponse.Success();
    }
}