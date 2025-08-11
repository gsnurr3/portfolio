using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Presentation.Common;

namespace RESTfulAPI.Application.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PatientsController(IMediator _mediator) : ControllerBase
    {
        [HttpGet(Name = "GetPatients")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Patient>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllPatients(CancellationToken cancellationToken)
        {
            var patientGetAllResponse = await _mediator.Send(new GetPatientsRequest(), cancellationToken);

            return Ok(ApiResponse.Ok(patientGetAllResponse));
        }
    }
}