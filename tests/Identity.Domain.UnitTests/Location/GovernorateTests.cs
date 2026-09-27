using Identity.Domain.Location;
using Identity.Tests.Common.Location;
using Xunit;

namespace Identity.Domain.UnitTests.Location;

public class GovernorateTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();

        var result = GovernorateFactory.CreateGovernorate(id: id, nameAr: "الجيزة", nameEn: "Giza");

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("الجيزة", result.Value.NameAr);
        Assert.Equal("Giza", result.Value.NameEn);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = Governorate.Create(Guid.Empty, "الجيزة", "Giza");

        Assert.True(result.IsError);
        Assert.Equal(GovernorateErrors.InvalidId.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidArabicName_ShouldFail(string nameAr)
    {
        var result = GovernorateFactory.CreateGovernorate(nameAr: nameAr);

        Assert.True(result.IsError);
        Assert.Equal(GovernorateErrors.InvalidArabicName.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEnglishName_ShouldFail(string nameEn)
    {
        var result = GovernorateFactory.CreateGovernorate(nameEn: nameEn);

        Assert.True(result.IsError);
        Assert.Equal(GovernorateErrors.InvalidEnglishName.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateNames_WithValidData_ShouldSucceed()
    {
        var governorate = GovernorateFactory.CreateGovernorate().Value;

        var result = governorate.UpdateNames("الاسكندرية", "Alexandria");

        Assert.True(result.IsSuccess);
        Assert.Equal("الاسكندرية", governorate.NameAr);
        Assert.Equal("Alexandria", governorate.NameEn);
    }

    [Fact]
    public void UpdateNames_WithInvalidArabicName_ShouldFail()
    {
        var governorate = GovernorateFactory.CreateGovernorate().Value;

        var result = governorate.UpdateNames(" ", "Alexandria");

        Assert.True(result.IsError);
        Assert.Equal(GovernorateErrors.InvalidArabicName.Code, result.TopError.Code);
    }
}
