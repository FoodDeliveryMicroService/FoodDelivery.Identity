using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AddressManagement.Commands.UpdateAddress;

public sealed class UpdateAddressCommandHandler(
    IAppDbContext context,
    ILogger<UpdateAddressCommandHandler> logger)
    : IRequestHandler<UpdateAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        UpdateAddressCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId, cancellationToken);

        if (address is null)
            return AddressErrors.NotFound;

        if (address.CustomerId != command.CustomerId)
            return AddressErrors.Unauthorized;

        var city = await context.Cities
            .FirstOrDefaultAsync(c => c.Id == request.CityId, cancellationToken);

        if (city is null)
            return AddressErrors.InvalidCity;

        if (city.GovernorateId != request.GovernorateId)
            return AddressErrors.InvalidLocation;

        var governorate = await context.Governorates
            .FirstOrDefaultAsync(g => g.Id == request.GovernorateId, cancellationToken);

        if (governorate is null)
            return AddressErrors.InvalidGovernorate;

        var updateResult = address.Update(
            request.Label,
            request.GovernorateId,
            request.CityId,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.Landmark,
            request.Latitude,
            request.Longitude);

        if (updateResult.IsError)
            return updateResult.Errors;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateLabel(ex))
        {
            logger.LogWarning(
                "Duplicate address label '{Label}' for customer {CustomerId}",
                request.Label, command.CustomerId);

            return AddressErrors.DuplicateLabel;
        }

        logger.LogInformation(
            "Address {AddressId} updated for customer {CustomerId}",
            address.Id, command.CustomerId);

        return new AddressDto(
            address.Id,
            address.Label,
            governorate.Id,
            governorate.NameAr,
            governorate.NameEn,
            city.Id,
            city.NameAr,
            city.NameEn,
            address.Street,
            address.BuildingNumber,
            address.Floor,
            address.Apartment,
            address.Landmark,
            address.Latitude,
            address.Longitude,
            address.IsDefault);
    }

    private static bool IsDuplicateLabel(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains(
            "IX_Addresses_CustomerId_Label",
            StringComparison.OrdinalIgnoreCase) == true;
}