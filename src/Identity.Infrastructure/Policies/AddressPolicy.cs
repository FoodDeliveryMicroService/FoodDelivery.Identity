using Identity.Application.Common.Interfaces;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Policies;

public sealed class AddressPolicy(IOptions<AddressSettings> options) : IAddressPolicy
{
    public int MaxAddressesPerCustomer => options.Value.MaxAddressesPerCustomer;
}