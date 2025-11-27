namespace Talapker.Infrastructure;

public record LocalizedText(string En = "", string Ru = "", string Kk = "")
{
    public string Resolve(string lang)
    {
        return lang switch
        {
            "kk" => Kk,
            "ru" => Ru,
            "en" => En,
            _ => Kk
        };
    }   
}