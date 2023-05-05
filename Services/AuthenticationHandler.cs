using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectApp.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ServerApp.Controllers.RegController;

namespace ServerApp.Services
{
   
    public class AuthenticationHandler
    {
        private readonly UsersDataContext _userContext;
        private readonly ILogger<AuthenticationHandler> _logger;
        public AuthenticationHandler(UsersDataContext userContext, ILogger<AuthenticationHandler> logger)
        {
            _userContext = userContext;
            _logger = logger;
        }
        public async Task<object> AuthenticateAsync(User user)
        {

            

            try
            {
                Console.WriteLine("Authentification work!");
              
                User? userData = await _userContext.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName && u.Password == user.Password);
                
                if (userData is null)
                {
                    _logger.LogWarning("Invalid username or password.");
                    return Results.Unauthorized();
                }
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName) };
                var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(1440)), // время действия 2 минуты
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new
                {
                    access_token = encodedJwt,
                    userId= userData.Id,
                    username = userData.Email,
                    password = userData.Password,
                };
                
                return Results.Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Results.BadRequest();
            }
        }
    }
}
