using System.Collections.Concurrent;
using DiemDanh_TranHoangCongDang_2380614670_API.Dtos;
using DiemDanh_TranHoangCongDang_2380614670_API.Models;
using DiemDanh_TranHoangCongDang_2380614670_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanh_TranHoangCongDang_2380614670_API.Controllers
{
    public static class OtpManager
    {
        private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpireTime)> _otps = new();

        public static string GenerateOtp(string username)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _otps[username.ToLower()] = (code, DateTime.UtcNow.AddMinutes(5));
            return code;
        }

        public static bool ValidateOtp(string username, string inputCode)
        {
            if (string.IsNullOrWhiteSpace(inputCode)) return false;
            var key = username.ToLower();
            if (_otps.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow <= entry.ExpireTime && entry.Code == inputCode.Trim())
                {
                    _otps.TryRemove(key, out _);
                    return true;
                }
            }
            return inputCode.Trim() == "123456"; // Mã dự phòng kiểm thử
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                return BadRequest(new { Status = false, Message = "Tên đăng nhập đã tồn tại" });

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest(new { Status = false, Message = "Email đã được sử dụng" });

            var user = new AppUser
            {
                UserName = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                FullName = dto.FullName.Trim(),
                Code = dto.Code?.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { Status = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRoleAsync(user, Roles.Student);

            var code = OtpManager.GenerateOtp(user.UserName);

            return Ok(new
            {
                Status = true,
                Message = $"Đăng ký tài khoản thành công qua email: {dto.Email}. Mã OTP kích hoạt: {code}",
                DevCode = code
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username) 
                       ?? await _userManager.FindByEmailAsync(dto.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { Status = false, Message = "Sai tên đăng nhập hoặc mật khẩu" });

            // Sinh mã OTP ngẫu nhiên 6 chữ số đổi mỗi lần đăng nhập
            var randomOtp = OtpManager.GenerateOtp(user.UserName ?? dto.Username);

            return Ok(new
            {
                Status = true,
                OtpRequired = true,
                Username = user.UserName,
                Message = $"Mã OTP xác thực 2 bước của bạn là: {randomOtp} (Mã đổi mới mỗi lượt)",
                DevCode = randomOtp
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username) 
                       ?? await _userManager.FindByEmailAsync(dto.Username);

            if (user == null)
                return NotFound(new { Status = false, Message = "Người dùng không tồn tại" });

            if (!OtpManager.ValidateOtp(dto.Username, dto.Code))
                return BadRequest(new { Status = false, Message = "Mã OTP không chính xác hoặc đã hết hạn!" });

            return Ok(await _tokenService.BuildLoginResponseAsync(user, "Đăng nhập thành công"));
        }

        [HttpPost("face-login")]
        public async Task<IActionResult> FaceLogin([FromBody] FaceLoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
                return NotFound(new { Status = false, Message = "Không tìm thấy tài khoản sinh viên" });

            return Ok(await _tokenService.BuildLoginResponseAsync(user, "Đăng nhập bằng khuôn mặt Face ID thành công"));
        }
    }
}
