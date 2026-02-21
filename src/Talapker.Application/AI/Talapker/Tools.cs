using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.AI.Talapker;

public class UniversityTools(TalapkerDbContext db)
{
    [Description("ПОИСК специальностей по коду или названию. Возвращает полную информацию включая баллы для получения гранта из конкурса грантов прошлого года.")]
public async Task<string> FindProgram(
    [Description("Список запросов: коды специальностей (например: '6В01102') или названия на любом языке.")] List<string> queries)
{
    var allPrograms = new List<EducationProgram>();

    foreach (var query in queries)
    {
        var byCode = await db.EducationPrograms
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.FirstSubject)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.SecondSubject)
            .Where(p => p.Code == query)
            .ToListAsync();

        var byName = await db.EducationPrograms
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.FirstSubject)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.SecondSubject)
            .Where(p =>
                p.Name.Ru.Contains(query) ||
                p.Name.Kk.Contains(query) ||
                p.Name.En.Contains(query))
            .ToListAsync();

        allPrograms.AddRange(byCode);
        allPrograms.AddRange(byName);
    }

    var programs = allPrograms.DistinctBy(p => p.Id).ToList();

    if (!programs.Any())
        return $"Специальности по запросам [{string.Join(", ", queries)}] не найдены.";

    var educationGroupIds = programs
        .Where(p => p.EducationGroupId.HasValue)
        .Select(p => p.EducationGroupId!.Value)
        .Distinct()
        .ToList();

    var grantStats = await db.GrantCompetitionStatistics
        .AsNoTracking()
        .Where(s => educationGroupIds.Contains(s.EducationGroupId))
        .OrderByDescending(s => s.Year)
        .ToListAsync();
    

    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Найдено {programs.Count} специальностей:");
    sb.AppendLine();

    foreach (var p in programs)
    {
        sb.AppendLine($"【{p.Code}】{p.Name.Ru} / {p.Name.Kk} / {p.Name.En}");
        sb.AppendLine($"  Описание: {p.Description.Ru}");
        sb.AppendLine($"  Места работы: {p.WorkPlaces.Ru}");
        sb.AppendLine($"  Базы практик: {p.PractiseBases.Ru}");
        sb.AppendLine($"  Факультет: {p.Faculty?.Name.Ru ?? "Не указан"}");
        sb.AppendLine($"  Мин. балл ЕНТ (университет): {p.MinimumUntScore}");
        sb.AppendLine($"  Языки обучения: {string.Join(", ", p.Languages.Select(l => l.ToString()))}");
        sb.AppendLine($"  Образовательная группа: {p.Code} - {p.EducationGroup?.Name.Ru ?? "Не указана"}");
        sb.AppendLine(
            $"  Связки предметов ЕНТ: {String.Join("; ", p.EducationGroup?.UntSubjectsPairs.Select(pair => $"{pair.FirstSubject?.Name.Ru} и {pair.SecondSubject?.Name.Ru}") ?? [])}");

        if (p.EducationGroupId.HasValue)
        {
            var groupStats = grantStats
                .Where(s => s.EducationGroupId == p.EducationGroupId)
                .GroupBy(s => s.Year)
                .OrderByDescending(g => g.Key)
                .Take(3);

            if (groupStats.Any())
            {
                sb.AppendLine($"  ГРАНТЫ (мин. баллы по годам):");
                foreach (var yearGroup in groupStats)
                {
                    var general = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.General);
                    var rural = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.Rural);
                    sb.Append($"    {yearGroup.Key}: ");
                    if (general != null) sb.Append($"общий — {general.MinScore}б  ");
                    if (rural != null) sb.Append($"сельский — {rural.MinScore}б");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine($"  ГРАНТЫ: статистика не найдена");
            }
        }

        sb.AppendLine();
    }
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(sb.ToString());
    Console.ResetColor();

    return sb.ToString();
}
}