using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
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
            .FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
        if (registeredUser != null){
            var token = GenerateJwtToken(registeredUser.UserId);
            Console.WriteLine($"Token for {user.Username} has been generated");
            return Ok(token);
        }
        else{
            Console.WriteLine("Token not generated");
            return Ok("Ok");
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