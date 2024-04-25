using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Server.Data;


[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManagementContext _context;

    public UsersController(UserManagementContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public ActionResult Register([FromBody] RegisterDto registerDto)
    {
        if (_context.Users.Any(u => u.Email == registerDto.Email))
        {
            return BadRequest("User already exists with the provided email.");
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = hashedPassword,
            Role = "User"  
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User registered successfully.");
    }
}

