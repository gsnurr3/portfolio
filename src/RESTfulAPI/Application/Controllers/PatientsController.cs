using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Presentation.Common;
using RESTfulAPI.Presentation.Responses;

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

        /// <summary>Create a patient</summary>
        /// <remarks>
        /// Sample:
        /// {
        ///   "medicalRecordNumber": "MRN001011",
        ///   "firstName": "Jane",
        ///   "lastName": "Miller",
        ///   "dateOfBirth": "1985-04-12",
        ///   "gender": "Female",
        ///   "address": "123 Main St, Springfield, IL",
        ///   "phoneNumber": "(555)555-1234",
        ///   "email": "jane.miller@example.com",
        ///   "insuranceProviderId": 2,
        ///   "insurancePolicyNumber": "POL123456"
        /// }
        /// </remarks>
        [HttpPost(Name = "PostPatient")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<PostPatientResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostPatient([FromBody] PostPatientRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            // Location header to the new resource (don’t have GetById yet, so use a conventional URL)
            var location = $"/api/patients/{result.Patient.PatientId}";
            Response.Headers.Location = location;

            return StatusCode(StatusCodes.Status201Created, ApiResponse.Created(result, location));
        }
    }
}