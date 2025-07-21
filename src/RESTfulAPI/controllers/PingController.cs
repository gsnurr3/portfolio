using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    // GET /api/ping  → 200 OK  "pong"
    [HttpGet]
    public ActionResult<string> Get() => "pong";
}