using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.AddressManagement.Queries.GetAddress;

public sealed class GetAddressQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAddressQuery, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        GetAddressQuery query,
        CancellationToken cancellationToken)
    {
        var result = await (
            from a in context.Addresses
            join g in context.Governorates on a.GovernorateId equals g.Id
            join c in context.Cities on a.CityId equals c.Id
            where a.Id == query.AddressId
            select new { Address = a, Governorate = g, City = c })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return AddressErrors.NotFound;

        if (result.Address.CustomerId != query.CustomerId)
            return AddressErrors.Unauthorized;

        return new AddressDto(
            result.Address.Id,
            result.Address.Label,
            result.Governorate.Id,
            result.Governorate.NameAr,
            result.Governorate.NameEn,
            result.City.Id,
            result.City.NameAr,
            result.City.NameEn,
            result.Address.Street,
            result.Address.BuildingNumber,
            result.Address.Floor,
            result.Address.Apartment,
            result.Address.Landmark,
            result.Address.Latitude,
            result.Address.Longitude,
            result.Address.IsDefault);
    }
}