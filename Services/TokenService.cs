using DiemDanh_TranHoangCongDang_2380614670_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiemDanh_TranHoangCongDang_2380614670_API.Services
{
    public class TokenService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public TokenService(UserManager<AppUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<object> BuildLoginResponseAsync(AppUser user, string message)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? Roles.Student;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("fullName", user.FullName),
                new Claim("code", user.Code ?? ""),
                new Claim(ClaimTypes.Role, role)
            };

            var jwt = _config.GetSection("JWTKey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = jwt["ValidIssuer"],
                Audience = jwt["ValidAudience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(handler.CreateToken(tokenDescriptor));

            return new
            {
                Status = true,
                Message = message,
                Token = token,
                Role = role,
                FullName = user.FullName,
                Code = user.Code
            };
        }
    }
}
