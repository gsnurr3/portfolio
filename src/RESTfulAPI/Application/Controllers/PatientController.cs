using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Presentation.Common;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PatientController(IMediator _mediator) : ControllerBase
    {
        [HttpGet]
        [Produces(typeof(ApiResponse<PatientGetAllResponse>))]
        public async Task<IActionResult> GetAllPatients(CancellationToken cancellationToken)
        {
            var patientGetAllResponse = await _mediator.Send(new PatientGetAllRequest(), cancellationToken);

            return patientGetAllResponse is null
                ? NotFound(ApiResponse.NotFound())
                : Ok(ApiResponse.Ok(patientGetAllResponse));
        }
    }
}