namespace Talapker.TelegramBot.Localization;

public record Localization
{
    public string WelcomeMessage  { get; init; } = "";
    public string ResetMessage    { get; init; } = "";
    public string HelpMessage     { get; init; } = "";
    public string ErrorMessage    { get; init; } = "";
    public string GrantsPrompt    { get; init; } = "";
    public string ProgramsPrompt  { get; init; } = "";
    public string DocumentsPrompt { get; init; } = "";
    public string DormitoryPrompt { get; init; } = "";
    public string DeadlinesPrompt { get; init; } = "";
    public string ContactsPrompt  { get; init; } = "";
    public string TuitionPrompt   { get; init; } = "";

    public string CommandPrograms  { get; init; } = "";
    public string CommandGrants    { get; init; } = "";
    public string CommandDocuments { get; init; } = "";
    public string CommandDormitory { get; init; } = "";
    public string CommandDeadlines { get; init; } = "";
    public string CommandContacts  { get; init; } = "";
    public string CommandTuition   { get; init; } = "";
    public string CommandLanguage  { get; init; } = "";
    public string CommandHelp      { get; init; } = "";
}