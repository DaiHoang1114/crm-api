using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCRM.Client.Dto;
using MyCRM.Client.Services;
using System.Security.Claims;

namespace MyCRM.Client.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(KeycloakService keycloakService) : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicUser()
        => Ok(new { message = "This is public user data" });

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var success = await keycloakService.UpdateUserAsync(userId, request);

        if (!success)
        {
            return BadRequest(new { message = "Failed to update user profile" });
        }

        return Ok(new { message = "Profile updated successfully" });
    }
}