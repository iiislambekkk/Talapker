namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class FieldSeed
{
    public string FieldCode { get; set; } = string.Empty;
    public string FieldDescriptionEn { get; set; } = string.Empty;
    public string FieldDescriptionKk { get; set; } = string.Empty;
    public string FieldDescriptionRu { get; set; } = string.Empty;
    public List<GroupSeed> Groups { get; set; } = new List<GroupSeed>();
}