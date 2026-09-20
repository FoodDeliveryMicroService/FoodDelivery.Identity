using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Commands.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid AddressId, Guid CustomerId)
    : IRequest<Result<Updated>>;