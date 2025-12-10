namespace Talapker.Infrastructure.Data.Institution;

public class UntSubject
{
    public Guid Id { get; set; }
    public int? SeedId { get; set; }
    public LocalizedText Name { get; set; } = new LocalizedText();
}