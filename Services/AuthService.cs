using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectApp.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServerApp.Services
{
    public class AuthService
    {

        private readonly UsersDataContext _context;

        public AuthService(UsersDataContext context)
        {
            _context = context;
        }

        public async Task<string?> Authenticate(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == username && x.Password == password);
            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: "your_issuer_here",
                audience: "your_audience_here",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
