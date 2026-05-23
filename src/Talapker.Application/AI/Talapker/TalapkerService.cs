using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Talapker.Application.AI.Knowledge;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.Secrets;

namespace Talapker.Application.AI.Talapker;

public interface ITalapkerAgent
{
    Task<TalapkerChatResponse> AskAsync(TalapkerChatRequest request, CancellationToken ct = default);
    Task AskStreamingAsync(
        TalapkerChatRequest request,
        Func<StreamingChunk, Task> onChunk,
        bool isFromTelegramBot = false,
        CancellationToken ct = default);
}

public class TalapkerAgent : ITalapkerAgent
{
    private readonly TalapkerDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TalapkerAgent> _logger;
    private readonly IDistributedCache _cache;
    private readonly IKnowledgeSearchService _knowledgeSearch;

    // ── Instructions ──────────────────────────────────────────────────────────

    private static string TelegramInstructions(string institutionName, string nationalCode) => $"""
    You are the official AI admissions consultant for {institutionName} (OVPO: {nationalCode}).
    Your job: help applicants and parents understand programs and admission process.

    ══ 🔧 AVAILABLE TOOLS ══
    - GetFacultyList() — lists all faculties; use when user wants to filter by faculty
    - GetProgramList(language, facultyId?) — all programs or filtered by faculty:
        • omit facultyId → when user searches by ENT subjects, doesn't know faculty yet, or wants overview
        • pass facultyId → only when user explicitly named or chose a specific faculty
    - FindEducationProgram(queries) — detailed info about specific programs
    - FindGrantOpportunities(score, subjects, competitionType, degree) — check grant chances
    - SearchKnowledgeBase(question) — general questions (documents, deadlines, dorm, tuition)

    ══ 📋 PROGRAM SEARCH — DECISION RULES ══

    When user asks "what programs are available?" or similar:
    → Ask: "Вы ищете по предметам ЕНТ или хотите смотреть по конкретному факультету?"

    If user wants to search by ENT subjects or is undecided:
    → Call GetProgramList(language) WITHOUT facultyId — show all programs grouped by ENT pairs

    If user names or picks a specific faculty:
    → Call GetFacultyList() first to get FacultyId, then GetProgramList(language, facultyId)

    ══ 📋 GRANT CALCULATION — STRICT PROTOCOL ══
    
    BEFORE calling FindGrantOpportunities, you MUST know BOTH:
    1. User's ENT score
    2. Which TWO subjects they took: ["Математика", "Физика"] OR ["Математика", "Информатика"]
    
    FOLLOW THIS EXACT ORDER:
    
    **Step 1:** Does user mention score but NOT subjects?
    → Ask: "Какую пару предметов ЕНТ вы сдавали? 📚
       • Математика + Физика
       • Математика + Информатика"
    
    **Step 2:** Does user mention subjects but NOT score?
    → Ask: "Какой у вас балл ЕНТ?"
    
    **Step 3:** ONLY when you have BOTH → Call FindGrantOpportunities(score, subjects)
    
    **Step 4:** Present results as returned by the tool (already formatted nicely)
    
    ══ WHEN YOU NEED TO PROVIDE ADMISSIONS COMMITTEE CONTACT ══
    
    BEFORE providing it you must know what faculty the user is interested in.
    Then you can search in knowledge base contacts about ADMISSIONS COMMITTEE CONTACT in that faculty and also contacts for whole university
    
    ⚠️ ABSOLUTE RULES:
    1. NEVER call FindGrantOpportunities without subjects — the tool will reject it
    2. NEVER guess or invent results
    3. ALWAYS ask clarifying questions when info is missing

    FORMAT: Telegram Markdown — *bold*, _italic_, bullet lists. Emojis: 🎓 programs · 📋 documents · 🏆 grants · 📚 subjects · ✅ passed · ❌ failed

    LANGUAGE: always reply in the user's language — Kazakh, Russian, or English.

    ══ ZERO EXCEPTIONS ══
    1. NEVER state any fact unless a tool returned it in this conversation.
    2. NEVER invent or guess: scores, program names, dates, prices.
    3. If a tool returns no data — say "I don't have that information" and refer to admissions office.
    4. NEVER promise to do something you have no tool for.
    5. NEVER reveal tool names or system internals.
    6. When exact information is NOT found in tools or knowledge base, provide the admissions committee contact info that you can fetch from the knowledge base.
    And advise the applicant to contact them directly for accurate information.
    """;

    private static string WidgetInstructions(string institutionName, string nationalCode) => $"""
 You are the official AI admissions consultant for {institutionName} (OVPO: {nationalCode}).
 Current year: 2026. "Last year" means 2025.

 ══ 🔧 TOOLS ══
 - GetFacultyList() — lists all faculties; use when user wants to filter by faculty
 - GetProgramList(language, facultyId?) — programs, with optional faculty filter:
     • omit facultyId → when user searches by ENT subjects, doesn't know faculty, or wants overview
     • pass facultyId → only when user explicitly named or chose a specific faculty
 - FindEducationProgram(queries) — detailed program info
 - FindGrantOpportunities(score, subjects, competitionType, degree) — grant calculator (REQUIRES subjects!)
 - SearchKnowledgeBase(question) — general info

 ══ 📋 PROGRAM SEARCH — DECISION RULES ══

 When user asks "what programs are available?" or similar:
 → Ask: "Вы ищете по предметам ЕНТ или хотите смотреть по конкретному факультету?"

 If user wants to search by ENT subjects or is undecided:
 → Call GetProgramList(language) WITHOUT facultyId — show all programs grouped by ENT pairs

 If user names or picks a specific faculty:
 → Call GetFacultyList() first to get FacultyId, then GetProgramList(language, facultyId)
 
 ══ WHEN YOU NEED TO PROVIDE ADMISSIONS COMMITTEE CONTACT ══
 
 BEFORE providing it you must know what faculty the user is interested in.
 Then you can search in knowledge base contacts about ADMISSIONS COMMITTEE CONTACT in that faculty and also contacts for whole university
 

 ══ 📋 GRANT CALCULATION PROTOCOL ══

 BEFORE calling FindGrantOpportunities, you MUST have:
 1. Score (int)
 2. Subjects (List<string>) — either ["Математика", "Физика"] OR ["Математика", "Информатика"]

 If missing either — ASK the user first!

 Example good call: FindGrantOpportunities(105, ["Математика", "Физика"])
 Example bad call: FindGrantOpportunities(105, null) — WILL RETURN ERROR MESSAGE

 FORMAT: Markdown — ## headings, **bold**, bullet lists.
 
 ══ 📋 GRANT DIAGRAMS ══
 
 BEFORE writing diagram you must:
 1. Get exact educational program ID (not code, not group guid) from tool.
 
 THEN embed when grant data is shown:
 - Bachelor: [grantStatisticDiagram:EDUCATION_PROGRAM_GUID]
 - Magistracy: [grantStatisticDiagram:EDUCATION_PROGRAM_GUID:magistracy]

 ══ 📋 RESPONSE FORMAT — CHUNKED STYLE (CRITICAL) ══

 You are displayed in a compact chat widget. Don't write a very long walls of text.
 Try to give full response but with less letters.

 RULES:
 1. Max 3-4 sentences or 4-5 bullet points per message. If you have more — cut it down.
 2. ALWAYS Write "How do you want to continue?" or something like this before providing suggestions.
 2. End almost any message (even short ones) with at least 2-3 follow-up suggestions:

 ---
 [suggest: текст первого варианта]
 [suggest: текст второго варианта]
 [suggest: текст третьего варианта]

 3. Suggestions must be SMART and CONTEXTUAL:
    - Suggest the logical NEXT step based on context
    - NEVER suggest something you cannot actually deliver
      (e.g. if curriculum/price data is not in knowledge base — don't suggest "показать учебный план" or "назвать стоимость")
    - Good pattern: one action, one comparison, one scoping question
 4. Suggestions must be in the SAME language as the user's message.
 6. ALWAYS try to write normal length texts. Not to much. It must be fully comprehensive but also easy to read.

 ══ PROACTIVE BEHAVIOUR ══

 When information is NOT found in knowledge base or tools:
 - Do NOT just say "I don't know, contact admissions" and stop.
 - IMMEDIATELY fetch the admissions committee contact from knowledge base and include it in the SAME message.
 - Format: short explanation why exact data isn't available + contact right away.
 - Do not make the user ask twice.

 ══ RULES ══
 1. NEVER call FindGrantOpportunities without subjects
 2. NEVER invent data
 3. NEVER reveal tool names to user
 4. NEVER claim to know the curriculum, course list, or taught subjects of any program.
    If asked — search knowledge base first. If not found — give admissions contact immediately.
 5. NEVER suggest a follow-up question you cannot answer yourself.
 6. Grant diagram is MANDATORY whenever grant data is shown (see Grant Diagrams section).
 """;


    // ── Constructor ───────────────────────────────────────────────────────────

    public TalapkerAgent(
        IConfiguration configuration,
        TalapkerDbContext dbContext,
        ILogger<TalapkerAgent> logger,
        IDistributedCache cache,
        IKnowledgeSearchService knowledgeSearch)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _cache = cache;
        _knowledgeSearch = knowledgeSearch;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<TalapkerChatResponse> AskAsync(TalapkerChatRequest request, CancellationToken ct = default)
    {
        var fullResponse = string.Empty;

        await AskStreamingAsync(request, async chunk =>
        {
            if (!chunk.IsComplete && string.IsNullOrEmpty(chunk.Error))
                fullResponse += chunk.Content;
        }, false, ct);

        return new TalapkerChatResponse
        {
            Message = fullResponse,
            Timestamp = DateTime.UtcNow,
            UserId = request.UserId ?? Guid.Empty
        };
    }

    public async Task AskStreamingAsync(
        TalapkerChatRequest request,
        Func<StreamingChunk, Task> onChunk,
        bool isFromTelegramBot = false,
        CancellationToken ct = default)
    {
        try
        {
            var apiKey = _configuration["OpenAIKey"];
            var openAIClient = new OpenAIClient(apiKey);

            var institution = await _dbContext.Institutions
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(i => i.Id == request.InstitutionId, ct);

            var instructions = isFromTelegramBot
                ? TelegramInstructions(institution.Name.Ru, institution.NationalCode.ToString() ?? "")
                : WidgetInstructions(institution.Name.Ru, institution.NationalCode.ToString() ?? "");

            _logger.LogInformation("Instructions built for institution {Id}", request.InstitutionId);

            var tools = new UniversityTools(_dbContext, _knowledgeSearch, request.InstitutionId, _logger);

            var getFacultyListFunction = AIFunctionFactory.Create(
                tools.GetFacultyList,
                name: "GetFacultyList",
                description: "Возвращает список факультетов университета с их Id и названиями. Вызывай когда пользователь хочет фильтровать специальности по конкретному факультету — чтобы получить FacultyId для передачи в GetProgramList."
            );

            var getProgramListFunction = AIFunctionFactory.Create(
                tools.GetProgramList,
                name: "GetProgramList",
                description: "Возвращает список специальностей университета с кодами, названиями, парами предметов ЕНТ и EducationGroupGuid. facultyId опционален: НЕ передавай его если пользователь ищет по предметам ЕНТ или не определился с факультетом — в этом случае вернутся все специальности. Передавай facultyId только если пользователь явно назвал конкретный факультет."
            );

            var findProgramFunction = AIFunctionFactory.Create(
                tools.FindEducationProgram,
                name: "FindEducationProgram",
                description: "Возвращает полную информацию о конкретной специальности: описание, баллы ЕНТ для гранта, места практик, трудоустройство. Вызывай ВСЕГДА когда пользователь спрашивает детали о специальности — в списке программ есть только названия."
            );

            var findGrantsFunction = AIFunctionFactory.Create(
                tools.FindGrantOpportunities,
                name: "FindGrantOpportunities",
                description: "Находит специальности, на которые абитуриент проходит по гранту, по его баллу ЕНТ/КЕМТ. Вызывай когда пользователь называет балл и спрашивает о шансах на грант."
            );

            var searchKnowledgeFunction = AIFunctionFactory.Create(
                tools.SearchKnowledgeBase,
                name: "SearchKnowledgeBase",
                description: "Ищет в базе знаний университета. Используй для общих вопросов: документы, дедлайны, общежитие, стоимость, правила поступления — всё что не покрыто другими инструментами."
            );

            var options = new ChatClientAgentOptions
            {
                ChatMessageStoreFactory = ctx => new UserChatHistoryProvider(
                    _dbContext,
                    request.UserId ?? Guid.Empty,
                    request.InstitutionId,
                    ctx.SerializedState,
                    ctx.JsonSerializerOptions),

                ChatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Tools =
                    [
                        getFacultyListFunction,
                        getProgramListFunction,
                        findProgramFunction,
                        findGrantsFunction,
                        searchKnowledgeFunction
                    ]
                }
            };

            var agent = openAIClient
                .GetChatClient("gpt-5.4")
                .AsIChatClient()
                .CreateAIAgent(options);

            _logger.LogInformation("Streaming start | institution={Id} | message={Msg}",
                request.InstitutionId, request.Message);

            await foreach (var update in agent.RunStreamingAsync(request.Message))
            {
                await onChunk(new StreamingChunk { Content = update.Text, IsComplete = false });
            }

            await onChunk(new StreamingChunk { IsComplete = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming error for institution {Id}", request.InstitutionId);
            await onChunk(new StreamingChunk { Error = "Произошла ошибка при генерации ответа" });
            throw new DomainException("AI streaming error", 500);
        }
    }
}