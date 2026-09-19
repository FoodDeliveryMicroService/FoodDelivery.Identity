using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AddressManagement.Queries.ListAddresses;

public sealed record ListAddressesQuery(Guid CustomerId) : IRequest<Result<List<AddressDto>>>;