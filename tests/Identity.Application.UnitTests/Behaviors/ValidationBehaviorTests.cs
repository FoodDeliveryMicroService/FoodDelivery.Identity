using FluentValidation;
using FluentValidation.Results;
using Identity.Application.Common.Behaviors;
using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Common.Results;
using MediatR;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly IValidator<AddAddressCommand> _validator = Substitute.For<IValidator<AddAddressCommand>>();
    private readonly RequestHandlerDelegate<Result<AddressDto>> _next = Substitute.For<RequestHandlerDelegate<Result<AddressDto>>>();

    private static AddAddressCommand CreateCommand() =>
        new(new AddAddressRequest("Home", Guid.NewGuid(), Guid.NewGuid(), "St", "1", null, null, null, null, null, false));

    private static Result<AddressDto> CreateSuccessResult() =>
        new AddressDto(
            Guid.NewGuid(), "Home", Guid.NewGuid(), "محافظة", "Governorate",
            Guid.NewGuid(), "مدينة", "City", "St", "1", null, null, null, null, null, false);

    [Fact]
    public async Task Handle_WhenValid_ShouldInvokeNext()
    {
        var behavior = new ValidationBehavior<AddAddressCommand, Result<AddressDto>>(_validator);
        var command = CreateCommand();
        var expected = CreateSuccessResult();

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        _next.Invoke().Returns(expected);

        var result = await behavior.Handle(command, _next, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenInvalid_ShouldReturnErrorsWithoutInvokingNext()
    {
        var behavior = new ValidationBehavior<AddAddressCommand, Result<AddressDto>>(_validator);
        var command = CreateCommand();

        var failures = new List<ValidationFailure> { new("Label", "Label is required.") };
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult(failures));

        var result = await behavior.Handle(command, _next, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Label", result.TopError.Code);
        Assert.Equal("Label is required.", result.TopError.Description);
        await _next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_WhenNoValidatorRegistered_ShouldInvokeNext()
    {
        var behavior = new ValidationBehavior<AddAddressCommand, Result<AddressDto>>();
        var command = CreateCommand();
        var expected = CreateSuccessResult();

        _next.Invoke().Returns(expected);

        var result = await behavior.Handle(command, _next, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
