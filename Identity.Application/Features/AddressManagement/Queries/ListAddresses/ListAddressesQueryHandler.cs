using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.AddressManagement.Queries.ListAddresses;

public sealed class ListAddressesQueryHandler(IAppDbContext context)
    : IRequestHandler<ListAddressesQuery, Result<List<AddressDto>>>
{
    public async Task<Result<List<AddressDto>>> Handle(
        ListAddressesQuery query,
        CancellationToken cancellationToken)
    {
        var addresses = await (
            from a in context.Addresses
            join g in context.Governorates on a.GovernorateId equals g.Id
            join c in context.Cities on a.CityId equals c.Id
            where a.CustomerId == query.CustomerId
            orderby a.IsDefault descending, a.Label
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
            .ToListAsync(cancellationToken);

        return addresses;
    }
}