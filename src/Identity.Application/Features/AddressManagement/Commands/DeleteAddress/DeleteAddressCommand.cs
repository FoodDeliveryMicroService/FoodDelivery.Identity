using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Commands.DeleteAddress;

public sealed record DeleteAddressCommand(Guid AddressId, Guid CustomerId)
    : IRequest<Result<Deleted>>;