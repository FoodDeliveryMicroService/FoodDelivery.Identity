using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.AddressManagement.Queries.ResolveAddressById;

public sealed class ResolveAddressByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<ResolveAddressByIdQuery, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        ResolveAddressByIdQuery query,
        CancellationToken cancellationToken)
    {
        var result = await (
            from a in context.Addresses
            join g in context.Governorates on a.GovernorateId equals g.Id
            join c in context.Cities on a.CityId equals c.Id
            where a.Id == query.AddressId && a.CustomerId == query.CustomerId
            select new AddressDto(
                a.Id,
                a.Label,
                g.Id,
                g.NameAr,
                g.NameEn,
                c.Id,
                c.NameAr,
                c.NameEn,
                a.Street,
                a.BuildingNumber,
                a.Floor,
                a.Apartment,
                a.Landmark,
                a.Latitude,
                a.Longitude,
                a.IsDefault))
            .FirstOrDefaultAsync(cancellationToken);

        return result is null ? AddressErrors.NotFound : result;
    }
}