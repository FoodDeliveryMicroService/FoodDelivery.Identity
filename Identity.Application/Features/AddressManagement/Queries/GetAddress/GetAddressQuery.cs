using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Queries.GetAddress;

public sealed record GetAddressQuery(Guid AddressId, Guid CustomerId) : IRequest<Result<AddressDto>>;