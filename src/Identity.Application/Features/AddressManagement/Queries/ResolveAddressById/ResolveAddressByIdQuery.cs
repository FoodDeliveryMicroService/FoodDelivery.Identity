using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Queries.ResolveAddressById;

public sealed record ResolveAddressByIdQuery(Guid AddressId, Guid CustomerId) : IRequest<Result<AddressDto>>;