using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.Data;
using ProductCatalog.DTOs;
using ProductCatalog.Models;

namespace ProductCatalog.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepo _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepo userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        // Validate the DTO minimally - null/empty creds are a 401, not a 500.
        if (string.IsNullOrWhiteSpace(loginDto.Username) ||
            string.IsNullOrWhiteSpace(loginDto.Password))
        {
            throw new UnauthorizedAccessException("Username and password are required.");
        }

        User? user = await _userRepo.GetByUsernameAsync(loginDto.Username);

        // One branch, one message. We DO NOT tell the caller which of the two
        // conditions failed (user not found vs wrong password) - that leaks
        // whether an account exists and enables username enumeration.
        if (user is null || user.PasswordHash != loginDto.Password)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return BuildToken(user);
    }

    private TokenResponseDTO BuildToken(User user)
    {
        // Claims are the "things we know about you" that ride inside the JWT.
        // Anyone who can read the token can read these - DO NOT put secrets here.
        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role)
        };

        string jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key missing from config.");
        string jwtIssuer = _config["Jwt:Issuer"]!;
        string jwtAudience = _config["Jwt:Audience"]!;

        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        DateTime expires = DateTime.UtcNow.AddHours(1);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer:             jwtIssuer,
            audience:           jwtAudience,
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        string serialized = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponseDTO
        {
            Token = serialized,
            ExpiresAt = expires
        };
    }

}
