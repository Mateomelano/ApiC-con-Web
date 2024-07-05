using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly string _key;

    // Simulación de almacenamiento de usuarios en memoria
    private static Dictionary<string, (string Hash, string Id, string Email, string Role)> users =
        new Dictionary<string, (string Hash, string Id, string Email, string Role)>();

    public AuthController(IConfiguration config)
    {
        _key = config.GetValue<string>("Jwt:Key");
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegister user)
    {
        if (users.ContainsKey(user.Username))
            return Conflict("Username already exists");

        var hashedPassword = PasswordHasher.HashPassword(user.Password);
        users[user.Username] = (hashedPassword, Guid.NewGuid().ToString(), user.Email, user.Role);

        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLogin user)
    {
        if (user == null || !users.ContainsKey(user.Username) ||
            !PasswordHasher.VerifyPassword(users[user.Username].Hash, user.Password))
            return Unauthorized();

        var userId = users[user.Username].Id;
        var email = users[user.Username].Email;
        var role = users[user.Username].Role;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Microsoft.IdentityModel.Json.JsonConvert
        //TypeLoadException: Could not load type 'Microsoft.IdentityModel.Json.JsonConvert' from assembly 'Microsoft.IdentityModel.Tokens, Version=7.6.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'.

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}

public class UserRegister
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

public class UserLogin
{
    public string Username { get; set; }
    public string Password { get; set; }
}