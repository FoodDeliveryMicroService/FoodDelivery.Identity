using System.Globalization;
using System.Reflection;
using CsvHelper;
using Identity.Domain.Location;
using Identity.Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Data.Seeders;

public static class LocationDataSeeder
{
    private const string GovernoratesResourceName = "Identity.Infrastructure.SeedData.governorates.csv";
    private const string CitiesResourceName = "Identity.Infrastructure.SeedData.cities.csv";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await dbContext.Governorates.AnyAsync())
        {
            logger.LogInformation("Governorates already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Starting seeding of Governorates and Cities...");

        var governorateCsv = ReadEmbeddedResource<GovernoratesCsvRecord>(GovernoratesResourceName);
        var governorateIdMap = new Dictionary<int, Guid>();

        var governorates = new List<Governorate>();
        foreach (var record in governorateCsv)
        {
            var result = Governorate.Create(
                Guid.NewGuid(),
                record.NameAr.Trim(),
                record.NameEn.Trim()
            );

            if (result.IsSuccess)
            {
                governorates.Add(result.Value);
                governorateIdMap[record.Id] = result.Value.Id;
            }
            else
            {
                logger.LogWarning($"Failed to create governorate: {record.NameAr} - {result.TopError.Description}");
            }
        }

        await dbContext.Governorates.AddRangeAsync(governorates);
        await dbContext.SaveChangesAsync();

        // 2. قراءة المدن
        var cityCsv = ReadEmbeddedResource<CitiesCsvRecord>(CitiesResourceName);
        var cities = new List<City>();

        foreach (var record in cityCsv)
        {
            if (!governorateIdMap.TryGetValue(record.GovernorateId, out var govId))
            {
                logger.LogWarning($"Governorate with ID {record.GovernorateId} not found for city: {record.NameAr}");
                continue;
            }

            var result = City.Create(
                Guid.NewGuid(),
                govId,
                record.NameAr.Trim(),
                record.NameEn.Trim()
            );

            if (result.IsSuccess)
            {
                cities.Add(result.Value);
            }
            else
            {
                logger.LogWarning($"Failed to create city: {record.NameAr} - {result.TopError.Description}");
            }
        }

        await dbContext.Cities.AddRangeAsync(cities);
        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Seeded {governorates.Count} governorates and {cities.Count} cities.");
    }

    private static IEnumerable<T> ReadEmbeddedResource<T>(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }
}