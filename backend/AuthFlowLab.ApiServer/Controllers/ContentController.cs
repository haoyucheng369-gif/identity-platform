using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthFlowLab.ApiServer.Controllers;

[ApiController]
[Route("content")]
public class ContentController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
        => Ok("Public content");

    [HttpGet("user")]
    [Authorize]
    public IActionResult UserContent()
        => Ok("User or authenticated service content");

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminContent()
        => Ok("Admin content");

    [HttpGet("read")]
    [Authorize(Policy = "ContentRead")]
    public IActionResult ReadContent()
        => Ok("Content read allowed");

    [HttpPost("write")]
    [Authorize(Policy = "ContentWrite")]
    public IActionResult WriteContent()
        => Ok("Content write allowed");

    [HttpGet("service")]
    [Authorize(Policy = "ServiceOnly")]
    public IActionResult ServiceContent()
        => Ok("Service-only content");
}
