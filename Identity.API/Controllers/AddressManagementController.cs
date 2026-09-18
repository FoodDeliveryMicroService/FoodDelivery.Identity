using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
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

        public AddressManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Add a new delivery address for the authenticated customer (FR-10)</summary>
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
    }
}