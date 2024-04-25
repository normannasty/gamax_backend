using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Server.DTOs;
using UserManagementApi.Server.Data;
using Microsoft.AspNetCore.Authorization;



[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManagementContext _context;
    

    public AuthController(IConfiguration configuration, UserManagementContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("login")]
    public ActionResult<string> Login([FromBody] UserLoginDto userLogin)
    {
        var user = AuthenticateUser(userLogin);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var tokenString = GenerateJWTToken(user);
        return Ok(new { Token = tokenString });
    }
    [HttpGet("userdata")]
    [Authorize]
    public IActionResult GetUserData()
    {
        var userName = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
        var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (userRole == "Admin")
        {
            var users = _context.Users.Select(u => new UserDto
            {
                Name = u.Name,
                Role = u.Role,
                Email = u.Email,
                
            }).ToList();
            return Ok(users); 
        }
        else
        {
            var user = _context.Users.FirstOrDefault(u => u.Name == userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var userDto = new UserDto
            {
                Name = user.Name,
                Role = user.Role,
                Email = user.Email,
                
            };
            return Ok(new List<UserDto> { userDto }); 
        }
    }




    [HttpGet("test-claims")]
    public IActionResult TestClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }

    private UserDto AuthenticateUser(UserLoginDto userLogin)
    {
        var user = _context.Users.SingleOrDefault(u => u.Name == userLogin.Name);

        
        if (user != null && BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
        {
            return new UserDto { Name = user.Name, Role = user.Role };
        }

        return null;
    }


    private string GenerateJWTToken(UserDto user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("Name", user.Name)
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

