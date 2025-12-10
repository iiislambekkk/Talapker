using System.Text.Json;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;
using Talapker.Institutions.Infrastructure.Data.Seeding.Models;

namespace Talapker.Infrastructure.Data.Seeding;


public class SeedData(TalapkerDbContext context)
{
    public async Task Seed()
    {
        SeedCities(context);
        SeedUntSubjects(context);
        SeedUntPairs(context);
        SeedClassifications(context);
        SeedStatistics(context);
    }


    private static void SeedCities(TalapkerDbContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var basePath = Path.Combine(AppContext.BaseDirectory, "SeedingData");
        
        var citiesString = File.ReadAllText(Path.Combine(basePath, "cities.json"));
        
        var citiesDeserialized = JsonSerializer.Deserialize<List<CitySeed>>(citiesString, options) ?? new List<CitySeed>();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Starting city seeding, found {citiesDeserialized.Count} cities in JSON.");
        
        if (!context.Cities.Any())
        {
            foreach (var city in citiesDeserialized)
            {
                var newCity = new City
                {
                    Name = new LocalizedText(city.NameEn, city.NameRu, city.NameKk),
                    Id = Guid.NewGuid(),
                };
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Adding city: " + city.NameEn);
                
                
                context.Cities.Add(newCity);
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Cities seeded.");
            context.SaveChanges();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Cities already seeded, skipping...");
        }
        
        Console.ResetColor();
    }
    
    private static void SeedUntSubjects(TalapkerDbContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var basePath = Path.Combine(AppContext.BaseDirectory, "SeedingData");
        
        var untSubjectsString = File.ReadAllText(Path.Combine(basePath, "subjects.json"));
        
        var untSubjectsDeserialized = JsonSerializer.Deserialize<List<UntSubjectSeed>>(untSubjectsString, options) ?? new List<UntSubjectSeed>();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Starting subjects seeding, found {untSubjectsDeserialized.Count} subjects in JSON.");

        var cities = context.Cities.ToList();
        
        if (!context.UntSubjects.Any())
        {
            foreach (var subject in untSubjectsDeserialized)
            {
              
                var newSubject = new UntSubject()
                {
                    Name = new LocalizedText(subject.NameEn, subject.NameRu, subject.NameKk),
                    Id = Guid.NewGuid(),
                    SeedId = subject.Id
                };
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Adding subject: " + subject.NameRu);
                
                context.UntSubjects.Add(newSubject);
            }
            
            Console.WriteLine("Subjects seeded.");
            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("Subjects already seeded, skipping...");
        }
        
        Console.ResetColor();
    }
    
    private static void SeedUntPairs(TalapkerDbContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var basePath = Path.Combine(AppContext.BaseDirectory, "SeedingData");

        var pairsString = File.ReadAllText(Path.Combine(basePath, "subjectPairsForUNT.json"));

        var pairsDeserialized = JsonSerializer.Deserialize<List<UntPairSeed>>(pairsString, options) 
                                ?? new List<UntPairSeed>();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Starting UNT pairs seeding, found {pairsDeserialized.Count} pairs in JSON.");

        var subjects = context.UntSubjects.ToList();

        if (!context.UntPairs.Any())
        {
            foreach (var pair in pairsDeserialized)
            {
                var firstSubject = subjects.FirstOrDefault(s => s.SeedId == pair.FirstSubject);
                var secondSucject = subjects.FirstOrDefault(s => s.SeedId == pair.SecondSubject);
                
                if (firstSubject == null || secondSucject == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Skipping pair {pair.Code} because one of the subjects does not exist.");
                    continue;
                }

                var newPair = new UntPair
                {
                    Id = Guid.NewGuid(),
                    SeedId = pair.Id,
                    FirstSubjectId = firstSubject.Id,
                    SecondSubjectId = secondSucject.Id,
                };

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Adding UNT pair: {pair.Code}");

                context.UntPairs.Add(newPair);
            }

            context.SaveChanges();
            Console.WriteLine("UNT pairs seeded.");
        }
        else
        {
            Console.WriteLine("UNT pairs already seeded, skipping...");
        }

        Console.ResetColor();
    }
    
    private static void SeedClassifications(TalapkerDbContext context)
{
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    var basePath = Path.Combine(AppContext.BaseDirectory, "SeedingData");
    var jsonString = File.ReadAllText(Path.Combine(basePath, "classification.json"));

    var classifications = JsonSerializer.Deserialize<List<ClassificationSeed>>(jsonString, options) 
                          ?? new List<ClassificationSeed>();

    // Берем только бакалавра
    var bachelor = classifications.FirstOrDefault(c => c.Id == 0);
    if (bachelor == null) return;

    if (!context.EducationDirections.Any())
    {
        foreach (var domain in bachelor.Domains)
        {
            var direction = new EducationDirection
            {
                Id = Guid.NewGuid(),
                Degree = Degree.Bachelor,
                Name = new LocalizedText(domain.DomainDescriptionEn, domain.DomainDescriptionRu, domain.DomainDescriptionKk)
            };


            foreach (var fieldSeed in domain.FieldsOfDomain)
            {
                var field = new EducationField
                {
                    Id = Guid.NewGuid(),
                    Name = new LocalizedText(fieldSeed.FieldDescriptionEn, fieldSeed.FieldDescriptionRu, fieldSeed.FieldDescriptionKk),
                    NationalCode = fieldSeed.FieldCode,
                    EducationDirection = direction
                };

                foreach (var groupSeed in fieldSeed.Groups)
                {
                    var group = new EducationGroup
                    {
                        Id = Guid.NewGuid(),
                        NationalCode = groupSeed.Code,
                        Name = new LocalizedText(groupSeed.DescriptionEn, groupSeed.DescriptionRu, groupSeed.DescriptionKk),
                        EducationField = field,
                        UntSubjectsPairs = context.UntPairs
                            .Where(p => groupSeed.SubjectPairsCodes.Contains(p.SeedId!.Value))
                            .ToList()
                    };

                    field.EducationGroups.Add(group);
                }

                direction.EducationFields.Add(field);
            }

            context.EducationDirections.Add(direction);
        }

        context.SaveChanges();
        Console.WriteLine("Classifications seeded (Bachelor only).");
    }
    else
    {
        Console.WriteLine("Classifications already seeded, skipping...");
    }
}

    private static void SeedStatistics(TalapkerDbContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    
        var basePath = Path.Combine(AppContext.BaseDirectory, "SeedingData", "Grants");
        var files = Directory.GetFiles(basePath, "*.json");

        if (context.GrantCompetitionStatistics.Any())
        {
            Console.WriteLine("Statistics already seeded, skipping...");
            return;
        }

        foreach (var statisticFile in files)
        {
            var jsonString = File.ReadAllText(statisticFile);
    
            var grantSeed = JsonSerializer.Deserialize<GrantSeed>(jsonString, options) 
                            ?? new GrantSeed();

            var eduGroupCode = Path.GetFileName(statisticFile)[..^5];
            var educationGroup = context.EducationGroups
                .FirstOrDefault(eg => eg.NationalCode == eduGroupCode); // Assuming file name is the education group code

            if (educationGroup == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Path.GetFileName(statisticFile)}: There is no education group with code {eduGroupCode}!!!");
                Console.ResetColor();
                continue;
            }

            var generalComp = grantSeed.General;
            if (generalComp != null)
            {
                var newStatisticEntity = new GrantCompetitionStatistic()
                {
                    Year = 2025,
                    Id = Guid.NewGuid(),
                    EducationGroup = educationGroup,
                    CompetitionType = GrantCompetitionType.General,
                    Records = generalComp.Records.Select(r => new GrantCompetitionRecord()
                    {
                        Score = r.Score,
                        UniversityCode = r.Ovpo
                    }).ToList(),
                    MinScore = generalComp.Records.Min(r => r.Score)
                }; 
            
                context.GrantCompetitionStatistics.Add(newStatisticEntity);
            }

            var ruralComp = grantSeed.Rural;
            if (ruralComp != null)
            {
                var newStatisticEntity = new GrantCompetitionStatistic()
                {
                    Year = 2025,
                    Id = Guid.NewGuid(),
                    EducationGroup = educationGroup,
                    CompetitionType = GrantCompetitionType.Rural,
                    Records = ruralComp.Records.Select(r => new GrantCompetitionRecord()
                    {
                        Score = r.Score,
                        UniversityCode = r.Ovpo
                    }).ToList(),
                    MinScore = ruralComp.Records.Min(r => r.Score)
                }; 
            
                context.GrantCompetitionStatistics.Add(newStatisticEntity);
            } 
        }
        
        context.SaveChanges();
        Console.WriteLine("GRANTS seeded.");
    }
}