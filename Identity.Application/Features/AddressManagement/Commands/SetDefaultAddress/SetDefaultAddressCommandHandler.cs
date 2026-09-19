using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AddressManagement.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressCommandHandler(
    IAppDbContext context,
    ILogger<SetDefaultAddressCommandHandler> logger)
    : IRequestHandler<SetDefaultAddressCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        SetDefaultAddressCommand command,
        CancellationToken cancellationToken)
    {
        var target = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId, cancellationToken);

        if (target is null)
            return AddressErrors.NotFound;

        if (target.CustomerId != command.CustomerId)
            return AddressErrors.Unauthorized;

        if (target.IsDefault)
            return AddressErrors.AlreadyDefault;

        var currentDefault = await context.Addresses
            .FirstOrDefaultAsync(
                a => a.CustomerId == command.CustomerId && a.IsDefault && a.Id != target.Id,
                cancellationToken);

        if (currentDefault is not null)
        {
            var removeResult = currentDefault.RemoveDefault();
            if (removeResult.IsError)
                return removeResult.Errors;
        }

        var setResult = target.SetDefault();
        if (setResult.IsError)
            return setResult.Errors;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Address {AddressId} set as default for customer {CustomerId}",
            target.Id, command.CustomerId);

        return Result.Updated;
    }
}