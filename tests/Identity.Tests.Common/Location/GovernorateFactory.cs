using Identity.Domain.Common.Results;
using Identity.Domain.Location;

namespace Identity.Tests.Common.Location;

public static class GovernorateFactory
{
    public static Result<Governorate> CreateGovernorate(
        Guid? id = null,
        string? nameAr = null,
        string? nameEn = null)
    {
        return Governorate.Create(
            id ?? Guid.NewGuid(),
            nameAr ?? "القاهرة",
            nameEn ?? "Cairo");
    }
}
