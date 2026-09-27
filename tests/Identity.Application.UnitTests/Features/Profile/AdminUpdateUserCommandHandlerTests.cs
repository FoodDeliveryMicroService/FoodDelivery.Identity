using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Profile.Commands.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Identity.Errors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Profile;

/// <summary>
/// The distinguishing feature versus UpdateProfile is the audit trail:
/// a successful admin update MUST write an audit log entry, a failed one
/// MUST NOT.
/// </summary>
public class AdminUpdateUserCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuditLogService _auditLogService = Substitute.For<IAuditLogService>();
    private readonly AdminUpdateUserCommandHandler _sut;

    public AdminUpdateUserCommandHandlerTests()
    {
        _sut = new AdminUpdateUserCommandHandler(
            _identityService, _auditLogService, Substitute.For<ILogger<AdminUpdateUserCommandHandler>>());
    }

    [Fact]
    public async Task Handle_WhenIdentityUpdateFails_ShouldNotWriteAuditLog()
    {
        var targetUserId = Guid.NewGuid();
        var command = new AdminUpdateUserCommand(targetUserId, new AdminUpdateUserRequest("New Name", null));

        _identityService.UpdateProfileAsync(targetUserId, "New Name", null, Arg.Any<CancellationToken>())
            .Returns(ProfileErrors.UserNotFound);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        await _auditLogService.DidNotReceive().LogAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenIdentityUpdateSucceeds_ShouldWriteAuditLogWithTargetUserAndAction()
    {
        var targetUserId = Guid.NewGuid();
        var command = new AdminUpdateUserCommand(targetUserId, new AdminUpdateUserRequest("New Name", "01099999999"));
        var updatedProfile = new ProfileDto(targetUserId, "user@example.com", "New Name", "01099999999", ["Customer"]);

        _identityService.UpdateProfileAsync(targetUserId, "New Name", "01099999999", Arg.Any<CancellationToken>())
            .Returns(updatedProfile);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.Name);

        await _auditLogService.Received(1).LogAsync(
            targetUserId,
            "ProfileUpdatedByAdmin",
            Arg.Any<string?>(),
            Arg.Is<string?>(newValue => newValue!.Contains("New Name") && newValue.Contains("01099999999")),
            Arg.Any<CancellationToken>());
    }
}