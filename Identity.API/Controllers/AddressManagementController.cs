using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Commands.DeleteAddress;
using Identity.Application.Features.AddressManagement.Commands.SetDefaultAddress;
using Identity.Application.Features.AddressManagement.Commands.UpdateAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.UpdateAddress;
using Identity.Application.Features.AddressManagement.Queries.GetAddress;
using Identity.Application.Features.AddressManagement.Queries.ListAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class AddressManagementController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly IUser _currentUser;

        public AddressManagementController(IMediator mediator, IUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>Add a new delivery address for the authenticated customer (FR-10/FR-11)</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddAddress(
            [FromBody] AddAddressRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AddAddressCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: address => CreatedEnvelope(address, "Address added successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>List the authenticated customer's saved addresses (FR-14)</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var result = await _mediator.Send(new ListAddressesQuery(_currentUser.Id.Value), cancellationToken);

            return result.Match<IActionResult>(
                onValue: addresses => OkEnvelope(addresses, "Addresses retrieved successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Get a single address owned by the authenticated customer (FR-15)</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAddress(Guid id, CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetAddressQuery(id, _currentUser.Id.Value), cancellationToken);

            return result.Match<IActionResult>(
                onValue: address => OkEnvelope(address, "Address retrieved successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Update one of the authenticated customer's addresses (FR-12)</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAddress(
            Guid id,
            [FromBody] UpdateAddressRequest request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var command = new UpdateAddressCommand(id, _currentUser.Id.Value, request);
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: address => OkEnvelope(address, "Address updated successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Delete one of the authenticated customer's addresses (FR-13)</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeleteAddressCommand(id, _currentUser.Id.Value), cancellationToken);

            return result.Match<IActionResult>(
                onValue: _ => OkEnvelope(new { }, "Address deleted successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Set one of the authenticated customer's addresses as default (FR-16)</summary>
        [HttpPut("{id:guid}/set-default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var result = await _mediator.Send(new SetDefaultAddressCommand(id, _currentUser.Id.Value), cancellationToken);

            return result.Match<IActionResult>(
                onValue: _ => OkEnvelope(new { }, "Default address updated successfully."),
                onError: errors => Problem(errors)
            );
        }
    }
}