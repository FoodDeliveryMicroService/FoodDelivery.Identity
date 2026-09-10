using CsvHelper.Configuration.Attributes;

namespace Identity.Infrastructure.SeedData;

public sealed class CitiesCsvRecord
{
    [Index(0)] public int Id { get; set; }
    [Index(1)] public int GovernorateId { get; set; }
    [Index(2)] public string NameAr { get; set; } = string.Empty;
    [Index(3)] public string NameEn { get; set; } = string.Empty;
}