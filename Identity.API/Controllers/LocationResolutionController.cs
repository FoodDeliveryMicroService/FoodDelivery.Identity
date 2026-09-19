using Identity.Application.Features.AddressManagement.Queries.ResolveAddressById;
using Identity.Application.Features.LocationResolution.Dtos.ResolveLocation;
using Identity.Application.Features.LocationResolution.Queries.ResolveCurrentLocation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class LocationResolutionController : ApiController
    {
        private readonly IMediator _mediator;

        public LocationResolutionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Resolve governorate/city from GPS coordinates (FR-09)</summary>
        [HttpGet("resolve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Resolve(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            CancellationToken cancellationToken)
        {
            var query = new ResolveCurrentLocationQuery(
                new ResolveLocationRequest(latitude, longitude));

            var result = await _mediator.Send(query, cancellationToken);

            return result.Match<IActionResult>(
                onValue: response => OkEnvelope(response, "Location resolved successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>
        /// Internal service-to-service endpoint (FR-17). Used by the Order Service
        /// to resolve a customer's AddressId into full address details at checkout.
        /// TODO: replace [Authorize(Policy = "InternalService")] with a proper
        /// service-to-service auth scheme (API key header or client-credentials JWT)
        /// once decided — don't leave this reachable by normal customer tokens.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{addressId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResolveAddress(
            Guid addressId,
            [FromQuery] Guid customerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ResolveAddressByIdQuery(addressId, customerId), cancellationToken);

            return result.Match<IActionResult>(
                onValue: address => OkEnvelope(address, "Address resolved successfully."),
                onError: errors => Problem(errors)
            );
        }
    }
}