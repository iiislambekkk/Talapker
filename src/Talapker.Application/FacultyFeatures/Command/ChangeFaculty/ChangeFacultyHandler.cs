using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.FacultyFeatures.Command.ChangeFaculty;

public class ChangeFacultyHandler()
{
    public async Task<ApiResponse> Handle(
        ChangeFacultyCommand command,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var faculty = await db.Faculties
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);
            
        if (faculty == null)
            return ApiResponse.Fail("Faculty not found", ErrorCodes.Default);
        
        // Обновляем поля
        faculty.Name = new LocalizedText
        {
            Kk = command.Name.GetValueOrDefault("kk", faculty.Name.Kk),
            Ru = command.Name.GetValueOrDefault("ru", faculty.Name.Ru),
            En = command.Name.GetValueOrDefault("en", faculty.Name.En)
        };
        
        if (command.LogoUrl != null)
            faculty.LogoUrl = command.LogoUrl;
            
        if (command.WallPaperUrl != null)
            faculty.WallPaperUrl = command.WallPaperUrl;
        
        await db.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Success();
    }
}