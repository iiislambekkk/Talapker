using Microsoft.AspNetCore.Http;

namespace Talapker.Infrastructure.Auth;

public interface ILanguageContext
{
    string Language { get; }
}

public class LanguageContext : ILanguageContext
{
    public string Language { get; }

    public LanguageContext(IHttpContextAccessor accessor)
    {
        var headers = accessor.HttpContext?.Request.GetTypedHeaders();

        var lang = headers?.AcceptLanguage?
            .OrderByDescending(x => x.Quality ?? 1)
            .FirstOrDefault()?.Value.ToString();

        Language = string.IsNullOrWhiteSpace(lang) ? "kk" : lang[..2];
    }
}