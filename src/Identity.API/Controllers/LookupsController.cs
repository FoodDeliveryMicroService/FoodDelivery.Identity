using Identity.Application.Features.GeographicLookup.Queries.ListCitiesByGovernorate;
using Identity.Application.Features.GeographicLookup.Queries.ListGovernorates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    /// <summary>Geographic reference data used for manual address creation (FR-18/FR-19)</summary>
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LookupsController : ApiController
    {
        private readonly IMediator _mediator;

        public LookupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>List all supported Governorates (FR-18)</summary>
        [HttpGet("governorates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGovernorates(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ListGovernoratesQuery(), cancellationToken);

            return result.Match<IActionResult>(
                onValue: governorates => OkEnvelope(governorates, "Governorates retrieved successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>List Cities belonging to a Governorate (FR-19)</summary>
        [HttpGet("cities/{governorateId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCities(Guid governorateId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ListCitiesByGovernorateQuery(governorateId), cancellationToken);

            return result.Match<IActionResult>(
                onValue: cities => OkEnvelope(cities, "Cities retrieved successfully."),
                onError: errors => Problem(errors)
            );
        }
    }
}