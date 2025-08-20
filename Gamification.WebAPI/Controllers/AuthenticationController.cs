using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.WebAPI.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase{
    private readonly IConfiguration _config;
    private readonly ProductivityDbContext _dbContext;
    
    public AuthenticationController(IConfiguration configuration, ProductivityDbContext dbContext){
        _config = configuration;
        _dbContext = dbContext;
    }
    
    [HttpPost("Login")]
    public IActionResult Login([FromBody] UserLogin user){
        User? registeredUser = _dbContext.Users
            .FirstOrDefault(u => user.Username == u.Username || user.Username == u.Email);
        if (registeredUser != null && BCrypt.Net.BCrypt.Verify(user.Password, registeredUser.Password)){
            var token = GenerateJwtToken(registeredUser.UserId);
            Console.WriteLine($"Token for {user.Username} has been generated");
            return Ok(token);
        }
        Console.WriteLine("Token not generated");
        return NotFound();
    }

    [HttpPost("Register")]
    public IActionResult Register([FromBody] UserRegister newUser){
        if (DoesUserExist(newUser.Username, newUser.Email)) return Conflict(new {message = "Username/Email is already taken."});
        User user = new User{
            Username = newUser.Username,
            Email = newUser.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password, 12),
            Goal = newUser.Goal
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        Console.WriteLine($"User password: {newUser.Password}, saved as : {user.Password}");
        
        return Ok("Registered");
        
        bool DoesUserExist(string username, string email){
            bool sameUsername = _dbContext.Users.Any(u => u.Username == username);
            bool sameEmail = _dbContext.Users.Any(u => u.Email == email);
            if (sameUsername || sameEmail) return true;
            return false;
        }
    }

    private string GenerateJwtToken(string userId){
        var claims = new[]{
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        string jwtKey = _config.GetValue<string>("JwtKeys:SymmetricKey") ?? throw new ArgumentNullException("JwtKeys:SymmetricKey");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7131",
            audience: "https://localhost:7131",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}