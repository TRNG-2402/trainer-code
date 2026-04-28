using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.DTOs;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers;

// [AllowAnonymous] is critical here. Once we add [Authorize] to the other
// controllers below, if we ever decided to apply a global authorization filter
// we'd lock ourselves out of the one endpoint needed to get a token.
// Explicit > implicit.
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/Auth/login
    // No try/catch - UnauthorizedAccessException bubbles to GlobalExceptionMiddleware
    // and becomes a proper 401 with the structured JSON body.
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDTO>> Login(LoginDTO loginDto)
    {
        return await _authService.LoginAsync(loginDto);
    }

}