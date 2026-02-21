using System.Text.Json;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;
using Talapker.Infrastructure.Data.Seeding.Models;
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
        SeedEducationPrograms(context);
    }
    
    private static void SeedEducationPrograms(TalapkerDbContext context)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Starting education programs seeding...");
    
    if (!context.EducationPrograms.Any())
    {
        // B01 - Музыка
        var musicProgram = new EducationProgram
        {
            Id = Guid.NewGuid(),
            Code = "6B01101",
            MinimumUntScore = 70,
            FacultyId = null,
            EducationGroupId = null,
            
            Name = new LocalizedText
            {
                Ru = "Музыкальное образование",
                Kk = "Музыкалық білім",
                En = "Music Education"
            },
            
            Description = new LocalizedText
            {
                Ru = "Подготовка педагогов-музыкантов",
                Kk = "Педагог-музыканттарды даярлау",
                En = "Training of music teachers"
            },
            
            WorkPlaces = new LocalizedText
            {
                Ru = "Школы, музыкальные школы, колледжи",
                Kk = "Мектептер, музыка мектептері, колледждер",
                En = "Schools, music schools, colleges"
            }
        };
        
        // B02 - Химия
        var chemistryProgram = new EducationProgram
        {
            Id = Guid.NewGuid(),
            Code = "6B01102",
            MinimumUntScore = 80,
            FacultyId = null,
            EducationGroupId = null,
            
            Name = new LocalizedText
            {
                Ru = "Химическое образование",
                Kk = "Химиялық білім",
                En = "Chemistry Education"
            },
            
            Description = new LocalizedText
            {
                Ru = "Подготовка учителей химии",
                Kk = "Химия мұғалімдерін даярлау",
                En = "Training of chemistry teachers"
            },
            
            WorkPlaces = new LocalizedText
            {
                Ru = "Школы, лицеи, колледжи, лаборатории",
                Kk = "Мектептер, лицейлер, колледждер, зертханалар",
                En = "Schools, lyceums, colleges, laboratories"
            }
        };
        
        // B03 - Математика
        var mathProgram = new EducationProgram
        {
            Id = Guid.NewGuid(),
            Code = "6B01103",
            MinimumUntScore = 75,
            FacultyId = null,
            EducationGroupId = null,
            
            Name = new LocalizedText
            {
                Ru = "Математическое образование",
                Kk = "Математикалық білім",
                En = "Mathematics Education"
            },
            
            Description = new LocalizedText
            {
                Ru = "Подготовка учителей математики",
                Kk = "Математика мұғалімдерін даярлау",
                En = "Training of mathematics teachers"
            },
            
            WorkPlaces = new LocalizedText
            {
                Ru = "Школы, лицеи, колледжи, IT-центры",
                Kk = "Мектептер, лицейлер, колледждер, IT-орталықтар",
                En = "Schools, lyceums, colleges, IT centers"
            }
        };
        
        context.EducationPrograms.AddRange(musicProgram, chemistryProgram, mathProgram);
        context.SaveChanges();
        
        Console.WriteLine($"Added 3 education programs with full data.");
    }
    else
    {
        Console.WriteLine("Education programs already seeded, skipping...");
    }
    
    Console.ResetColor();
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
                .FirstOrDefault(eg => eg.NationalCode == eduGroupCode);

            if (educationGroup == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Path.GetFileName(statisticFile)}: There is no education group with code {eduGroupCode}!!!");
                Console.ResetColor();
                continue;
            }

            if (grantSeed.General != null)
            {
                context.GrantCompetitionStatistics.Add(
                    BuildStatistic(educationGroup, GrantCompetitionType.General, grantSeed.General.Records)
                );
            }

            if (grantSeed.Rural != null)
            {
                context.GrantCompetitionStatistics.Add(
                    BuildStatistic(educationGroup, GrantCompetitionType.Rural, grantSeed.Rural.Records)
                );
            }
            }

        context.SaveChanges();
        Console.WriteLine("GRANTS seeded.");
    }

    private static GrantCompetitionStatistic BuildStatistic(
        EducationGroup educationGroup,
        GrantCompetitionType competitionType,
        List<RawRecord> rawRecords)
    {
        var scoreFrequencies = rawRecords
            .GroupBy(r => r.Score)
            .Select(g => new GrantCompetitionRecord
            {
                Score = g.Key,
                Frequency = g.Count()
            })
            .ToList();

        return new GrantCompetitionStatistic
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EducationGroup = educationGroup,
            CompetitionType = competitionType,
            Records = scoreFrequencies,
            MinScore = rawRecords.Min(r => r.Score),
            TotalGrants = rawRecords.Count
        };
    }
}