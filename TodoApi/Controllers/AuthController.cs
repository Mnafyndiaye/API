using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    private static readonly Dictionary<string, (string Password, string Role)> Users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = ("Admin123!", "admin"),
        ["user"] = ("User123!", "user")
    };

    public AuthController(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        if (!Users.TryGetValue(request.Username, out var userInfo) || userInfo.Password != request.Password)
        {
            return Unauthorized("Nom d'utilisateur ou mot de passe invalide.");
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, userInfo.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return Ok(new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = userInfo.Role,
            ExpiresAtUtc = expires
        });
    }
}
