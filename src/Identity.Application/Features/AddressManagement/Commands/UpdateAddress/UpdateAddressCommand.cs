using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Application.Features.AddressManagement.Dtos.UpdateAddress;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Commands.UpdateAddress;

public sealed record UpdateAddressCommand(Guid AddressId, Guid CustomerId, UpdateAddressRequest Request)
    : IRequest<Result<AddressDto>>;