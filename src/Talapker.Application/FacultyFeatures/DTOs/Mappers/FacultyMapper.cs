using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.DTOs.Mappers;


public static class FacultyMapper
{
    public static FacultyDto ToDto(this Faculty faculty)
    {
        return new FacultyDto
        {
            Id = faculty.Id,
            Name = faculty.Name,
            InstitutionId = faculty.InstitutionId,
            LogoUrl = faculty.LogoUrl,
            WallPaperUrl = faculty.WallPaperUrl,
            EducationPrograms = faculty.EducationPrograms.Select(ep => ep.ToDto()).ToList()
        };
    }
    
    public static List<FacultyDto> ToDtoList(this IEnumerable<Faculty> faculties)
    {
        return faculties.Select(f => f.ToDto()).ToList();
    }
    
    public static FacultySlimDto ToSlimDto(this Faculty faculty) => new()
    {
        Id = faculty.Id,
        Name = faculty.Name,
        InstitutionId = faculty.InstitutionId,
        LogoUrl = faculty.LogoUrl,
        WallPaperUrl = faculty.WallPaperUrl
    };
}