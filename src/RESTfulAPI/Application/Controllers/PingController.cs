using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;
    public PingController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Produces(typeof(ApiResponse<PingResponse>))]
    public async Task<IActionResult> Ping([FromBody] PingRequest request, CancellationToken cancellationToken)
    {
        var pingResponse = await _mediator.Send(request, cancellationToken);

        return pingResponse is null
            ? NotFound(ApiResponse.NotFound())
            : Ok(ApiResponse.Ok(pingResponse));
    }
}