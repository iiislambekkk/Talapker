using System.ComponentModel;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Application.AI.Knowledge;
using Talapker.Infrastructure;

namespace Talapker.Application.AI.Talapker;

public class UniversityTools(
    TalapkerDbContext db,
    IKnowledgeSearchService knowledgeSearch,
    Guid institutionId,
    ILogger<TalapkerAgent> logger)
{
    // ── GetFacultyList ────────────────────────────────────────────────────────

    [Description("Возвращает список факультетов университета с Id и названиями. Вызывай когда пользователь хочет фильтровать специальности по конкретному факультету.")]
    public async Task<string> GetFacultyList()
    {
        logger.LogInformation("[GetFacultyList] institutionId={Id}", institutionId);

        var faculties = await db.Faculties
            .AsNoTracking()
            .Where(f => f.InstitutionId == institutionId)
            .OrderBy(f => f.Name.Ru)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync();

        if (!faculties.Any())
            return "Факультеты не найдены.";

        var sb = new StringBuilder();
        sb.AppendLine($"ФАКУЛЬТЕТЫ ({faculties.Count}):");
        foreach (var f in faculties)
            sb.AppendLine($"  FacultyId={f.Id} | {f.Name.Ru} / {f.Name.Kk} / {f.Name.En}");

        logger.LogInformation("[GetFacultyList] returned {Count} faculties", faculties.Count);
        return sb.ToString();
    }

    // ── GetProgramList ────────────────────────────────────────────────────────

    [Description("Возвращает список специальностей университета с парами предметов ЕНТ. facultyId опционален — не передавай его если пользователь ищет по предметам или не определился с факультетом.")]
    public async Task<string> GetProgramList(
        [Description("Язык названий программ: 'ru', 'kk' или 'en'.")] string language = "ru",
        [Description("FacultyId из GetFacultyList. Передавай только если пользователь явно назвал конкретный факультет. Если не указан — возвращаются специальности всех факультетов.")] Guid? facultyId = null)
    {
        logger.LogInformation("[GetProgramList] lang={Lang}, facultyId={FacultyId}, institutionId={Id}",
            language, facultyId, institutionId);

        var query = db.EducationPrograms
            .AsNoTracking()
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
                .ThenInclude(g => g!.UntSubjectsPairs)
                .ThenInclude(pair => pair.FirstSubject)
            .Include(p => p.EducationGroup)
                .ThenInclude(g => g!.UntSubjectsPairs)
                .ThenInclude(pair => pair.SecondSubject)
            .Where(p => p.Faculty != null && p.Faculty.InstitutionId == institutionId);

        if (facultyId.HasValue)
            query = query.Where(p => p.Faculty!.Id == facultyId.Value);

        var programs = await query.OrderBy(p => p.Code).ToListAsync();

        if (!programs.Any())
            return facultyId.HasValue
                ? "На выбранном факультете специальности не найдены."
                : "Специальности не найдены.";

        string GetName(LocalizedText name) => language switch
        {
            "kk" => name.Kk,
            "en" => name.En,
            _    => name.Ru
        };

        string GetSubjectName(LocalizedText name) => language switch
        {
            "kk" => name.Kk,
            "en" => name.En,
            _    => name.Ru
        };

        var bachelor   = programs.Where(p => p.Code.StartsWith("6") || p.Code.StartsWith("6B")).ToList();
        var magistracy = programs.Where(p => p.Code.StartsWith("7") || p.Code.StartsWith("7M") || p.Code.StartsWith("M")).ToList();

        var sb = new StringBuilder();

        if (facultyId.HasValue)
        {
            var facultyName = programs.FirstOrDefault()?.Faculty?.Name;
            if (facultyName != null)
                sb.AppendLine($"ФАКУЛЬТЕТ: {GetName(facultyName)}");
            sb.AppendLine();
        }

        if (bachelor.Any())
        {
            sb.AppendLine($"БАКАЛАВРИАТ ({bachelor.Count} специальностей):");
            foreach (var p in bachelor)
            {
                var pairs = p.EducationGroup?.UntSubjectsPairs
                    .Select(pair => $"{GetSubjectName(pair.FirstSubject!.Name)} + {GetSubjectName(pair.SecondSubject!.Name)}")
                    .ToList() ?? [];

                var pairsStr = pairs.Any() ? $" | ЕНТ: {string.Join(" / ", pairs)}" : "";
                sb.AppendLine($"  EducationGroupGuid={p.EducationGroupId} | {p.Code} | {GetName(p.Name)}{pairsStr}");
            }
            sb.AppendLine();
        }

        if (magistracy.Any())
        {
            sb.AppendLine($"МАГИСТРАТУРА ({magistracy.Count} специальностей):");
            foreach (var p in magistracy)
                sb.AppendLine($"  EducationGroupGuid={p.EducationGroupId} | {p.Code} | {GetName(p.Name)}");
        }

        logger.LogInformation("[GetProgramList] returned bachelor={B}, magistracy={M}, facultyFilter={F}",
            bachelor.Count, magistracy.Count, facultyId?.ToString() ?? "none");

        return sb.ToString();
    }

    // ── FindEducationProgram ──────────────────────────────────────────────────

    [Description("ПОИСК специальностей по коду или названию. ВСЕГДА вызывай когда пользователь спрашивает о конкретной специальности. Возвращает полную информацию включая баллы для получения гранта.")]
    public async Task<string> FindEducationProgram(
        [Description("Список запросов: коды специальностей (например: '6В01102', 'M001') или названия на любом языке.")] List<string> queries)
    {
        logger.LogInformation("[FindProgram] Запрос: queries=[{Queries}], institutionId={InstitutionId}",
            string.Join(", ", queries), institutionId);

        var dbTask  = SearchProgramsInDb(queries);
        var ragTask = knowledgeSearch.SearchAsync(new KnowledgeSearchOptions(
            Question: string.Join(" ", queries),
            InstitutionId: institutionId,
            TopK: 20,
            ScoreThreshold: 0.45f
        ));

        await Task.WhenAll(dbTask, ragTask);

        var (programs, magistracyOnly, grantStats) = await dbTask;
        var ragResult = await ragTask;

        logger.LogInformation("[FindProgram] Найдено из БД: бакалавриат={BachelorCount}, магистратура={MagistracyCount}. RAG чанков: {RagCount}",
            programs.Count, magistracyOnly.Count, ragResult.Chunks.Count);

        var sb = new StringBuilder();

        if (programs.Any())
        {
            sb.AppendLine($"БАКАЛАВРИАТ — найдено {programs.Count} специальностей:");
            sb.AppendLine();

            foreach (var p in programs)
            {
                sb.AppendLine($"ID (GUID): {p.Id}");
                sb.AppendLine($"【{p.Code}】{p.Name.Ru} / {p.Name.Kk} / {p.Name.En}");
                sb.AppendLine($"  Описание: {p.Description.Ru}");
                sb.AppendLine($"  Места работы: {p.WorkPlaces.Ru}");
                sb.AppendLine($"  Базы практик: {p.PractiseBases.Ru}");
                sb.AppendLine($"  Факультет: {p.Faculty?.Name.Ru ?? "Не указан"}");
                sb.AppendLine($"  Мин. балл ЕНТ для поступления в наш университет на платное обучение (именно в наш ВУЗ, внутреннии пороговый балл): {p.MinimumPlatnoeUntScore}");
                sb.AppendLine($"  Мин. балл ЕНТ для поступления в наш университет на обучение по гранту (именно в наш ВУЗ, внутреннии пороговый балл): {p.MinimumGrantUntScore}");
                sb.AppendLine($"  Языки обучения: {string.Join(", ", p.Languages.Select(l => l.ToString()))}");
                sb.AppendLine($"  Образовательная группа: {p.EducationGroup?.NationalCode} - {p.EducationGroup?.Name.Ru ?? "Не указана"}");
                sb.AppendLine($"  Связки предметов ЕНТ: {string.Join("; ", p.EducationGroup?.UntSubjectsPairs.Select(pair => $"{pair.FirstSubject?.Name.Ru} и {pair.SecondSubject?.Name.Ru}") ?? [])}");

                if (p.EducationGroupId.HasValue)
                    AppendBachelorGrants(sb, grantStats, p.EducationGroupId.Value);

                sb.AppendLine();
            }
        }

        if (magistracyOnly.Any())
        {
            sb.AppendLine($"МАГИСТРАТУРА — найдено {magistracyOnly.Count} специальностей:");
            sb.AppendLine();

            foreach (var g in magistracyOnly)
            {
                sb.AppendLine($"【{g.NationalCode}】{g.Name.Ru} / {g.Name.Kk} / {g.Name.En}");
                AppendMagistracyGrants(sb, grantStats, g.Id);
                sb.AppendLine();
            }
        }

        if (!programs.Any() && !magistracyOnly.Any())
            sb.AppendLine($"Специальности по запросам [{string.Join(", ", queries)}] не найдены в базе данных.");

        if (ragResult.Chunks.Any())
        {
            sb.AppendLine("ДОПОЛНИТЕЛЬНАЯ ИНФОРМАЦИЯ ИЗ БАЗЫ ЗНАНИЙ:");
            sb.AppendLine();

            foreach (var chunk in ragResult.Chunks)
            {
                sb.AppendLine($"  В: {chunk.Question}");
                sb.AppendLine($"  О: {chunk.Answer}");
                sb.AppendLine();
            }
        }

        var result = sb.ToString();
        logger.LogInformation("[FindProgram → AI] Ответ:\n{Result}", result);
        return result;
    }

    // ── SearchKnowledgeBase ───────────────────────────────────────────────────

    [Description("ПОИСК в базе знаний учебного заведения по произвольному вопросу. Формулируй вопрос только на русском языке. Используй этот инструмент ТОЛЬКО если другие инструменты не применимы к вопросу пользователя — например, вопрос касается общей информации об университете, правил поступления, документов, дедлайнов, общежития и других тем, которые не покрываются специализированными инструментами.")]
    public async Task<string> SearchKnowledgeBase(
        [Description("Вопрос или тема для поиска в базе знаний на русском языке.")] string question)
    {
        logger.LogInformation("[SearchKnowledgeBase] Запрос: question={Question}, institutionId={InstitutionId}",
            question, institutionId);

        var result = await knowledgeSearch.SearchAsync(new KnowledgeSearchOptions(
            Question: question,
            InstitutionId: institutionId,
            TopK: 30,
            ScoreThreshold: 0.45f
        ));

        logger.LogInformation("[SearchKnowledgeBase] Найдено RAG чанков: {Count}", result.Chunks.Count);

        if (!result.Chunks.Any())
        {
            logger.LogWarning("[SearchKnowledgeBase] Ничего не найдено по запросу: {Question}", question);
            return $"По запросу «{question}» в базе знаний ничего не найдено.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"РЕЗУЛЬТАТЫ ИЗ БАЗЫ ЗНАНИЙ (запрос: «{question}»):");
        sb.AppendLine();

        foreach (var chunk in result.Chunks)
        {
            sb.AppendLine($"В: {chunk.Question}");
            sb.AppendLine($"О: {chunk.Answer}");
            sb.AppendLine($"  (релевантность: {chunk.Score:F2})");
            sb.AppendLine();
        }

        var response = sb.ToString();
        logger.LogDebug("[SearchKnowledgeBase → AI] Ответ:\n{Response}", response);
        return response;
    }

    // ── FindGrantOpportunities ────────────────────────────────────────────────

    [Description("Находит специальности, по которым абитуриент может претендовать на грант, исходя из его балла. Используй когда пользователь называет свой балл и хочет узнать, на какие специальности он проходит по гранту.")]
    public async Task<string> FindGrantOpportunities(
        [Description("Балл абитуриента (ЕНТ для бакалавриата, КЕМТ/ГЭ для магистратуры).")] int score,
        [Description("Тип конкурса: 'general' (общий), 'rural' (сельский), 'profile' (профильная магистратура), 'ped' (педагогическая магистратура), 'all' (все). По умолчанию 'all'.")] string competitionType = "all",
        [Description("Уровень образования: 'bachelor', 'magistracy', 'all'. По умолчанию 'all'.")] string degree = "all")
    {
        logger.LogInformation("[FindGrantOpportunities] score={Score}, competitionType={Type}, degree={Degree}, institutionId={InstitutionId}",
            score, competitionType, degree, institutionId);

        var degreeFilter = degree.ToLower() switch
        {
            "bachelor"   => new[] { GrantDegree.Bachelor },
            "magistracy" => new[] { GrantDegree.Magistracy },
            _            => new[] { GrantDegree.Bachelor, GrantDegree.Magistracy }
        };

        var typeFilter = competitionType.ToLower() switch
        {
            "general" => new[] { GrantCompetitionType.General },
            "rural"   => new[] { GrantCompetitionType.Rural },
            "profile" => new[] { GrantCompetitionType.Profile },
            "ped"     => new[] { GrantCompetitionType.Ped },
            _         => null // все типы
        };

        var query = db.GrantCompetitionStatistics
            .AsNoTracking()
            .Where(s => s.MinScore <= score && degreeFilter.Contains(s.Degree));

        if (typeFilter != null)
            query = query.Where(s => typeFilter.Contains(s.CompetitionType));

        var stats = await query
            .OrderByDescending(s => s.Year)
            .ToListAsync();

        if (!stats.Any())
            return $"По баллу {score} подходящих специальностей с грантами не найдено.";

        var latestPerGroup = stats
            .GroupBy(s => (s.EducationGroupId, s.CompetitionType, s.Degree))
            .Select(g => g.OrderByDescending(s => s.Year).First())
            .ToList();

        var groupIds = latestPerGroup.Select(s => s.EducationGroupId).Distinct().ToList();

        var bachelorPrograms = await db.EducationPrograms
            .AsNoTracking()
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
            .Where(p => p.Faculty != null
                     && p.Faculty.InstitutionId == institutionId
                     && p.EducationGroupId.HasValue
                     && groupIds.Contains(p.EducationGroupId!.Value))
            .Select(p => new { p.Code, p.Name, p.MinimumGrantUntScore, p.EducationGroupId })
            .ToListAsync();

        var magistracyGroups = await db.EducationGroups
            .AsNoTracking()
            .Where(g => g.NationalCode.StartsWith("M") && groupIds.Contains(g.Id))
            .Select(g => new { g.Id, g.NationalCode, g.Name })
            .ToListAsync();

        logger.LogInformation("[FindGrantOpportunities] Найдено: бакалавриат={B}, магистратура={M}",
            bachelorPrograms.Count, magistracyGroups.Count);

        var sb = new StringBuilder();
        sb.AppendLine($"СПЕЦИАЛЬНОСТИ, НА КОТОРЫЕ ПРОХОДИТ БАЛЛ {score}:");
        sb.AppendLine();

        // Бакалавриат
        var bachelorStats = latestPerGroup
            .Where(s => s.Degree == GrantDegree.Bachelor)
            .ToList();

        if (bachelorStats.Any())
        {
            var bachelorRows = bachelorStats
                .Join(bachelorPrograms,
                    s => s.EducationGroupId,
                    p => p.EducationGroupId,
                    (s, p) => new { Stat = s, Program = p })
                .DistinctBy(x => (x.Program.Code, x.Stat.CompetitionType))
                .OrderBy(x => x.Stat.MinScore)
                .ToList();

            if (bachelorRows.Any())
            {
                sb.AppendLine($"БАКАЛАВРИАТ ({bachelorRows.Count} специальностей):");
                foreach (var row in bachelorRows)
                {
                    var typeLabel = row.Stat.CompetitionType switch
                    {
                        GrantCompetitionType.General => "общий",
                        GrantCompetitionType.Rural   => "сельский",
                        _                            => row.Stat.CompetitionType.ToString()
                    };
                    sb.AppendLine($"  [{row.Program.Code}] {row.Program.Name.Ru} — мин. грант {row.Stat.MinScore}б ({typeLabel}, {row.Stat.Year})");
                }
                sb.AppendLine();
            }
        }

        // Магистратура
        var magistracyStats = latestPerGroup
            .Where(s => s.Degree == GrantDegree.Magistracy)
            .ToList();

        if (magistracyStats.Any() && magistracyGroups.Any())
        {
            var magistracyRows = magistracyStats
                .Join(magistracyGroups,
                    s => s.EducationGroupId,
                    g => g.Id,
                    (s, g) => new { Stat = s, Group = g })
                .OrderBy(x => x.Stat.MinScore)
                .ToList();

            if (magistracyRows.Any())
            {
                sb.AppendLine($"МАГИСТРАТУРА ({magistracyRows.Count} специальностей):");
                foreach (var row in magistracyRows)
                {
                    var typeLabel = row.Stat.CompetitionType switch
                    {
                        GrantCompetitionType.Profile => "профильная",
                        GrantCompetitionType.Ped     => "педагогическая",
                        _                            => row.Stat.CompetitionType.ToString()
                    };
                    sb.AppendLine($"  [{row.Group.NationalCode}] {row.Group.Name.Ru} — мин. грант {row.Stat.MinScore}б ({typeLabel}, {row.Stat.Year})");
                }
                sb.AppendLine();
            }
        }

        if (!bachelorPrograms.Any() && !magistracyGroups.Any())
            sb.AppendLine("Названия специальностей не найдены — возможно, данные ещё не заполнены.");

        return sb.ToString();
    }

    // ── Приватные методы ──────────────────────────────────────────────────────

    private async Task<(List<EducationProgram> Programs, List<EducationGroup> MagistracyOnly, List<GrantCompetitionStatistic> GrantStats)> SearchProgramsInDb(List<string> queries)
    {
        var allPrograms = new List<EducationProgram>();
        var allGroups   = new List<EducationGroup>();

        foreach (var query in queries)
        {
            var byCode = await db.EducationPrograms
                .Include(p => p.Faculty)
                .Include(p => p.EducationGroup).ThenInclude(g => g!.UntSubjectsPairs).ThenInclude(p => p.FirstSubject)
                .Include(p => p.EducationGroup).ThenInclude(g => g!.UntSubjectsPairs).ThenInclude(p => p.SecondSubject)
                .Where(p => p.Code == query)
                .AsNoTracking()
                .ToListAsync();

            var byName = await db.EducationPrograms
                .Include(p => p.Faculty)
                .Include(p => p.EducationGroup).ThenInclude(g => g!.UntSubjectsPairs).ThenInclude(p => p.FirstSubject)
                .Include(p => p.EducationGroup).ThenInclude(g => g!.UntSubjectsPairs).ThenInclude(p => p.SecondSubject)
                .Where(p => p.Name.Ru.Contains(query) || p.Name.Kk.Contains(query) || p.Name.En.Contains(query))
                .AsNoTracking()
                .ToListAsync();

            var magistracy = await db.EducationGroups
                .Where(g => g.NationalCode.StartsWith("M") && (
                    g.NationalCode == query ||
                    g.Name.Ru.Contains(query) ||
                    g.Name.Kk.Contains(query) ||
                    g.Name.En.Contains(query)))
                .AsNoTracking()
                .ToListAsync();

            allPrograms.AddRange(byCode);
            allPrograms.AddRange(byName);
            allGroups.AddRange(magistracy);
        }

        var programs       = allPrograms.DistinctBy(p => p.Id).ToList();
        var magistracyOnly = allGroups.DistinctBy(g => g.Id).ToList();

        var allGroupIds = programs
            .Where(p => p.EducationGroupId.HasValue)
            .Select(p => p.EducationGroupId!.Value)
            .Union(magistracyOnly.Select(g => g.Id))
            .Distinct()
            .ToList();

        var grantStats = allGroupIds.Count > 0
            ? await db.GrantCompetitionStatistics
                .AsNoTracking()
                .Where(s => allGroupIds.Contains(s.EducationGroupId))
                .OrderByDescending(s => s.Year)
                .ToListAsync()
            : [];

        return (programs, magistracyOnly, grantStats);
    }

    // ── Grant formatters ──────────────────────────────────────────────────────

    private static void AppendBachelorGrants(StringBuilder sb, List<GrantCompetitionStatistic> allStats, Guid groupId)
    {
        var groupStats = allStats
            .Where(s => s.EducationGroupId == groupId && s.Degree == GrantDegree.Bachelor)
            .GroupBy(s => s.Year)
            .OrderByDescending(g => g.Key)
            .Take(3)
            .ToList();

        if (!groupStats.Any()) { sb.AppendLine("  ГРАНТЫ: статистика не найдена"); return; }

        sb.AppendLine("  ГРАНТЫ (мин. баллы по годам):");
        foreach (var yearGroup in groupStats)
        {
            var general = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.General);
            var rural   = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.Rural);
            sb.Append($"    {yearGroup.Key}: ");
            if (general != null) sb.Append($"общий — {general.MinScore}б  ");
            if (rural   != null) sb.Append($"сельский — {rural.MinScore}б");
            sb.AppendLine();
        }
    }

    private static void AppendMagistracyGrants(StringBuilder sb, List<GrantCompetitionStatistic> allStats, Guid groupId)
    {
        var groupStats = allStats
            .Where(s => s.EducationGroupId == groupId && s.Degree == GrantDegree.Magistracy)
            .GroupBy(s => s.Year)
            .OrderByDescending(g => g.Key)
            .Take(3)
            .ToList();

        if (!groupStats.Any()) { sb.AppendLine("  ГРАНТЫ: статистика не найдена"); return; }

        sb.AppendLine("  ГРАНТЫ (мин. баллы по годам):");
        foreach (var yearGroup in groupStats)
        {
            var prof = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.Profile);
            var ped  = yearGroup.FirstOrDefault(s => s.CompetitionType == GrantCompetitionType.Ped);
            sb.Append($"    {yearGroup.Key}: ");
            if (prof != null) sb.Append($"профильная — {prof.MinScore}б  ");
            if (ped  != null) sb.Append($"педагогическая — {ped.MinScore}б");
            sb.AppendLine();
        }
    }
}