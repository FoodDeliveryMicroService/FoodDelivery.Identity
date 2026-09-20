namespace Identity.Infrastructure.Settings;

public sealed class AddressSettings
{
    public const string SectionName = "AddressSettings";

    public int MaxAddressesPerCustomer { get; set; } = 10;
}