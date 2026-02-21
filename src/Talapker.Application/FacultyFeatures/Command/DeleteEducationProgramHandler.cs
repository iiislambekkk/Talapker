using Microsoft.EntityFrameworkCore;
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
        CancellationToken cancellationToken)
    {
        var program = await db.EducationPrograms
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (program == null)
            return ApiResponse.Fail("Education program not found", ErrorCodes.Default);

        db.EducationPrograms.Remove(program);
        await db.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success();
    }
}