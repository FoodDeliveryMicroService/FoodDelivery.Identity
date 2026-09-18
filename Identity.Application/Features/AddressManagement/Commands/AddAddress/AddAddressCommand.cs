using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Commands.AddAddress;

public sealed record AddAddressCommand(AddAddressRequest Request)
    : IRequest<Result<AddressDto>>;