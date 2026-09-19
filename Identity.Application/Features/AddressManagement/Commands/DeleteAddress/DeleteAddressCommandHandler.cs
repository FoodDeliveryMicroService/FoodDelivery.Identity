using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AddressManagement.Commands.DeleteAddress;

public sealed class DeleteAddressCommandHandler(
    IAppDbContext context,
    ILogger<DeleteAddressCommandHandler> logger)
    : IRequestHandler<DeleteAddressCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(
        DeleteAddressCommand command,
        CancellationToken cancellationToken)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId, cancellationToken);

        if (address is null)
            return AddressErrors.NotFound;

        if (address.CustomerId != command.CustomerId)
            return AddressErrors.Unauthorized;

        context.Addresses.Remove(address);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Address {AddressId} deleted for customer {CustomerId}",
            address.Id, command.CustomerId);

        return Result.Deleted;
    }
}