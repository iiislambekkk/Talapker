namespace Talapker.Infrastructure.Data.Seeding.Models;

public class MagistracyGroupSeed
{
    public string Code { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameKk { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class MagistracyGrantRecordSeed
{
    public int Score { get; set; }
    public string Ovpo { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; 
}