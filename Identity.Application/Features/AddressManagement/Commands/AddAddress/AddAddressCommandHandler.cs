using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AddressManagement.Commands.AddAddress;

public sealed class AddAddressCommandHandler(
    IAppDbContext context,
    IUser currentUser,
    ILogger<AddAddressCommandHandler> logger)
    : IRequestHandler<AddAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(
        AddAddressCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
            return Error.Unauthorized("Address.Unauthorized", "User is not authenticated.");

        var customerId = currentUser.Id.Value;
        var request = command.Request;

        var city = await context.Cities
            .FirstOrDefaultAsync(c => c.Id == request.CityId, cancellationToken);

        if (city is null)
            return AddressErrors.InvalidCity;

        // FR-10 acceptance criteria: City لازم تكون تابعة للـ Governorate المختارة
        if (city.GovernorateId != request.GovernorateId)
            return AddressErrors.InvalidLocation;

        var governorate = await context.Governorates
            .FirstOrDefaultAsync(g => g.Id == request.GovernorateId, cancellationToken);

        if (governorate is null)
            return AddressErrors.InvalidGovernorate;

        var addressResult = Address.Create(
            Guid.NewGuid(),
            customerId,
            request.Label,
            request.GovernorateId,
            request.CityId,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.Landmark,
            request.Latitude,
            request.Longitude,
            request.IsDefault);

        if (addressResult.IsError)
            return addressResult.Errors;

        var address = addressResult.Value;

        // لو العنوان الجديد Default، لازم نشيل الـ Default القديم
        if (request.IsDefault)
        {
            var existingDefault = await context.Addresses
                .FirstOrDefaultAsync(
                    a => a.CustomerId == customerId && a.IsDefault,
                    cancellationToken);

            if (existingDefault is not null)
            {
                var removeResult = existingDefault.RemoveDefault();
                if (removeResult.IsError)
                    return removeResult.Errors;
            }
        }

        context.Addresses.Add(address);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateLabel(ex))
        {
            logger.LogWarning(
                "Duplicate address label '{Label}' for customer {CustomerId}",
                request.Label, customerId);

            return AddressErrors.DuplicateLabel;
        }

        logger.LogInformation(
            "Address {AddressId} created for customer {CustomerId}",
            address.Id, customerId);

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