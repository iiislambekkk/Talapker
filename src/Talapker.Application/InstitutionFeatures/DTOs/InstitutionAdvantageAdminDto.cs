using Talapker.Infrastructure;

namespace Talapker.Application.InstitutionFeatures.DTOs;

public class InstitutionAdvantageAdminDto
{
    public Guid Id { get; set; }
    public LocalizedText Title { get; set; } = new LocalizedText();
    public LocalizedText Description { get; set; }  = new LocalizedText();
}