using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiWithJwt.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _key;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _key = config.GetValue<string>("Jwt:Key");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegister user)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return Conflict("Username already exists");
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return Conflict("Username already exists");

        if (user.Password.Length <= 8)
            return BadRequest("Password must be longer than 8 characters");

        if (user.Role != "Administrador" && user.Role != "Usuario")
            return BadRequest("Role must be either 'Administrador' or 'Usuario'");


        var hashedPassword = PasswordHasher.HashPassword(user.Password);

        var newUser = new User
        {
            Username = user.Username,
            PasswordHash = hashedPassword,
            Email = user.Email,
            Role = user.Role
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin user)
    {
        var dbUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == user.Username);
        if (dbUser == null || !PasswordHasher.VerifyPassword(dbUser.PasswordHash, user.Password))
            return Unauthorized();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, dbUser.Username),
                new Claim("UserId", dbUser.Id.ToString()),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim(ClaimTypes.Role, dbUser.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")] 
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} does not exist" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
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