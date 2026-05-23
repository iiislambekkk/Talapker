namespace Talapker.TelegramBot.Localization;

public static class Localizations
{
    public static Localization Get(UserLanguage language) => language switch
    {
        UserLanguage.Kazakh  => Kazakh,
        UserLanguage.Russian => Russian,
        UserLanguage.English => English,
        _ => Kazakh,
    };
    
    public static string GetLangCode(UserLanguage language) => language switch
    {
        UserLanguage.Kazakh  => "kk",
        UserLanguage.Russian => "ru",
        UserLanguage.English => "en",
        _ => "kk",
    };

    private static readonly Localization Kazakh = new()
    {
        WelcomeMessage =
            "👋 *Қош келдіңіз!*\n\n" +
            "Мен — қабылдау комиссиясының ЖИ кеңесшісі.\n\n" +
            "Маған тікелей сұрақ қоя аласыз — мысалы:\n" +
            "• _«Менің ҰБТ балым 118, грантқа өте аламын ба?»_\n" +
            "• _«IT мамандықтары бар ма?»_\n" +
            "• _«Жатақхана бар ма, қанша тұрады?»_\n\n" +
            "Немесе / батырмасын басып командалар тізімін ашыңыз 👇",

        ResetMessage = "🔄 *Диалог тазаланды.* Жаңа сұрақ қоя аласыз!",
        ErrorMessage = "⚠️ Қате орын алды. Кейінірек қайталап көріңіз.",

        GrantsPrompt =
            "🏆 *Гранттар*\n\n" +
            "ҰБТ/КЕМТ балыңызды жазыңыз.\n" +
            "Мысалы: _«Менің балы 110»_\n\n" +
            "Мен қандай мамандықтарға грант алуға болатынын көрсетемін.",

        ProgramsPrompt  = "Факультет бойынша барлық мамандықтардың толық тізімін көрсетіңіз",
        DocumentsPrompt = "Какие документы нужны для поступления?",
        DormitoryPrompt = "Расскажи про общежитие: есть ли места, стоимость, условия",
        DeadlinesPrompt = "Какие сроки подачи документов для поступления?",
        ContactsPrompt  = "Контакты приёмной комиссии: адрес, телефон, email",
        TuitionPrompt   = "Сколько стоит обучение по специальностям?",

        HelpMessage =
            "ℹ️ *Ботты қалай пайдалану керек*\n\n" +
            "Жай ғана сұрағыңызды жазыңыз — бот өзі түсінеді.\n\n" +
            "*Жылдам командалар (/ басыңыз):*\n" +
            "/programs — мамандықтар\n" +
            "/grants — гранттар\n" +
            "/documents — құжаттар\n" +
            "/dormitory — жатақхана\n" +
            "/deadlines — мерзімдер\n" +
            "/contacts — байланыс\n" +
            "/tuition — оқу құны\n" +
            "/language — тілді өзгерту\n" +
            "*Мысалдар:*\n" +
            "_«115 балл, грантқа өте аламын ба?»_\n" +
            "_«Математика + Физика бар мамандықтар»_\n" +
            "_«Заң факультеті қанша тұрады?»_",

        CommandPrograms  = "📚 Мамандықтар тізімі",
        CommandGrants    = "🏆 Гранттар",
        CommandDocuments = "📋 Құжаттар",
        CommandDormitory = "🏠 Жатақхана",
        CommandDeadlines = "📅 Мерзімдер",
        CommandContacts  = "📞 Байланыс",
        CommandTuition   = "💰 Оқу құны",
        CommandLanguage  = "🌐 Тілді өзгерту",
        CommandHelp      = "ℹ️ Анықтама",
    };

    private static readonly Localization Russian = new()
    {
        WelcomeMessage =
            "👋 *Добро пожаловать!*\n\n" +
            "Я — ИИ-консультант приёмной комиссии.\n\n" +
            "Задайте вопрос напрямую — например:\n" +
            "• _«Мой балл ЕНТ 118, пройду на грант?»_\n" +
            "• _«Есть ли IT-специальности?»_\n" +
            "• _«Есть ли общежитие и сколько стоит?»_\n\n" +
            "Или нажмите / чтобы открыть список команд 👇",

        ResetMessage = "🔄 *Диалог очищен.* Задайте новый вопрос!",
        ErrorMessage = "⚠️ Произошла ошибка. Попробуйте позже.",

        GrantsPrompt =
            "🏆 *Гранты*\n\n" +
            "Напишите ваш балл ЕНТ/КЕМТ.\n" +
            "Например: _«Мой балл 110»_\n\n" +
            "Я покажу, на какие специальности вы проходите по гранту.",

        ProgramsPrompt  = "Покажи список специальностей университета, по определенному факультету.",
        DocumentsPrompt = "Какие документы нужны для поступления?",
        DormitoryPrompt = "Расскажи про общежитие: есть ли места, стоимость, условия",
        DeadlinesPrompt = "Какие сроки подачи документов для поступления?",
        ContactsPrompt  = "Контакты приёмной комиссии: адрес, телефон, email",
        TuitionPrompt   = "Сколько стоит обучение по специальностям?",

        HelpMessage =
            "ℹ️ *Как пользоваться ботом*\n\n" +
            "Просто напишите вопрос — бот поймёт сам.\n\n" +
            "*Быстрые команды (нажмите /):*\n" +
            "/programs — специальности\n" +
            "/grants — гранты\n" +
            "/documents — документы\n" +
            "/dormitory — общежитие\n" +
            "/deadlines — сроки\n" +
            "/contacts — контакты\n" +
            "/tuition — стоимость обучения\n" +
            "/language — сменить язык\n" +
            "*Примеры вопросов:*\n" +
            "_«115 баллов, пройду на грант?»_\n" +
            "_«Специальности с Математикой и Физикой»_\n" +
            "_«Сколько стоит юридический факультет?»_",

        CommandPrograms  = "📚 Список специальностей",
        CommandGrants    = "🏆 Гранты",
        CommandDocuments = "📋 Документы",
        CommandDormitory = "🏠 Общежитие",
        CommandDeadlines = "📅 Сроки",
        CommandContacts  = "📞 Контакты",
        CommandTuition   = "💰 Стоимость обучения",
        CommandLanguage  = "🌐 Сменить язык",
        CommandHelp      = "ℹ️ Справка",
    };

    private static readonly Localization English = new()
    {
        WelcomeMessage =
            "👋 *Welcome!*\n\n" +
            "I'm an AI admissions consultant.\n\n" +
            "Ask me anything directly — for example:\n" +
            "• _\"My ENT score is 118, can I get a grant?\"_\n" +
            "• _\"What IT specialties are available?\"_\n" +
            "• _\"Is there a dormitory and how much does it cost?\"_\n\n" +
            "Or press / to see the list of commands 👇",

        ResetMessage = "🔄 *Chat cleared.* Ask a new question!",
        ErrorMessage = "⚠️ An error occurred. Please try again later.",

        GrantsPrompt =
            "🏆 *Grants*\n\n" +
            "Tell me your ENT/KEMT score.\n" +
            "For example: _\"My score is 110\"_\n\n" +
            "I'll show you which specialties you qualify for.",

        ProgramsPrompt  = "Show the full list of all university specialties",
        DocumentsPrompt = "What documents are required for admission?",
        DormitoryPrompt = "Tell me about the dormitory: availability, cost, conditions",
        DeadlinesPrompt = "What are the application deadlines?",
        ContactsPrompt  = "Admissions office contacts: address, phone, email",
        TuitionPrompt   = "How much is tuition by specialty?",

        HelpMessage =
            "ℹ️ *How to use this bot*\n\n" +
            "Just type your question — the bot understands free text.\n\n" +
            "*Quick commands (press /):*\n" +
            "/programs — specialties\n" +
            "/grants — grants\n" +
            "/documents — documents\n" +
            "/dormitory — dormitory\n" +
            "/deadlines — deadlines\n" +
            "/contacts — contacts\n" +
            "/tuition — tuition fees\n" +
            "/language — change language\n" +
            "*Example questions:*\n" +
            "_\"Score 115, do I qualify for a grant?\"_\n" +
            "_\"Specialties with Math and Physics\"_\n" +
            "_\"How much is the law faculty?\"_",

        CommandPrograms  = "📚 List of specialties",
        CommandGrants    = "🏆 Grants",
        CommandDocuments = "📋 Documents",
        CommandDormitory = "🏠 Dormitory",
        CommandDeadlines = "📅 Deadlines",
        CommandContacts  = "📞 Contacts",
        CommandTuition   = "💰 Tuition fees",
        CommandLanguage  = "🌐 Change language",
        CommandHelp      = "ℹ️ Help",
    };
}