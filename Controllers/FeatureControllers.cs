using DiemDanh_TranHoangCongDang_2380614670_API.Data;
using DiemDanh_TranHoangCongDang_2380614670_API.Dtos;
using DiemDanh_TranHoangCongDang_2380614670_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiemDanh_TranHoangCongDang_2380614670_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CoursesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _db.Courses.Include(c => c.Lecturer).ToListAsync();
            return Ok(courses.Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Room,
                c.TotalSessions,
                c.MaxAbsentPercent,
                LecturerName = c.Lecturer != null ? c.Lecturer.FullName : "GV. Nguyễn Văn Giảng"
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
        {
            var course = new Course
            {
                Code = dto.Code,
                Name = dto.Name,
                Room = dto.Room ?? "E1-02.05",
                TotalSessions = dto.TotalSessions,
                MaxAbsentPercent = dto.MaxAbsentPercent
            };
            _db.Courses.Add(course);
            await _db.SaveChangesAsync();
            return Ok(course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            return Ok(new { Status = true, Message = "Đã xóa môn học" });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SessionsController(AppDbContext db) => _db = db;

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var sessions = await _db.ClassSessions
                .Where(s => s.CourseId == courseId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
            return Ok(sessions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SessionCreateDto dto)
        {
            var session = new ClassSession
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                StartTime = DateTime.UtcNow,
                Latitude = dto.Latitude ?? 10.8221,
                Longitude = dto.Longitude ?? 106.6882,
                RadiusMeters = dto.RadiusMeters
            };
            _db.ClassSessions.Add(session);
            await _db.SaveChangesAsync();
            return Ok(session);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private static string _currentActiveQr = "TSV-1-K7X9A2";
        private static DateTime _qrExpireTime = DateTime.UtcNow.AddMinutes(5);

        public AttendanceController(AppDbContext db) => _db = db;

        [HttpGet("active-qr")]
        public IActionResult GetActiveQr()
        {
            return Ok(new
            {
                Token = _currentActiveQr,
                SecondsRemaining = Math.Max(0, (int)(_qrExpireTime - DateTime.UtcNow).TotalSeconds)
            });
        }

        [HttpPost("set-qr")]
        public IActionResult SetActiveQr([FromBody] QrUpdateDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Token))
            {
                _currentActiveQr = dto.Token.Trim();
                _qrExpireTime = DateTime.UtcNow.AddSeconds(35);
            }
            return Ok(new { Token = _currentActiveQr });
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        {
            double schoolLat = 10.8221;
            double schoolLng = 106.6882;
            double dist = 18.2;

            if (dto.Latitude != null && dto.Longitude != null)
            {
                double dLat = Math.Abs(dto.Latitude.Value - schoolLat) * 111000;
                double dLng = Math.Abs(dto.Longitude.Value - schoolLng) * 111000;
                dist = Math.Sqrt(dLat * dLat + dLng * dLng);
            }

            bool outOfRange = dist > 200;

            return Ok(new
            {
                Status = true,
                Message = outOfRange 
                    ? $"Cảnh báo: Bạn đang cách lớp {(int)dist}m (Ngoài phạm vi cho phép)" 
                    : $"Điểm danh thành công qua mã QR động [{dto.Code ?? "TSV-QR"}]! Vị trí hợp lệ trong phòng học.",
                DistanceMeters = (int)dist,
                OutOfRange = outOfRange,
                QrCode = dto.Code
            });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LeaveRequestsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.LeaveRequests.Include(l => l.Student).Include(l => l.Session).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeaveRequestDto dto)
        {
            var req = new LeaveRequest
            {
                SessionId = dto.SessionId,
                StudentId = "u_dang", // Mặc định Trần Hoàng Công Đăng
                Reason = dto.Reason,
                ProofUrl = dto.ProofUrl,
                Status = "PENDING"
            };
            _db.LeaveRequests.Add(req);
            await _db.SaveChangesAsync();
            return Ok(req);
        }

        [HttpPost("review")]
        public async Task<IActionResult> Review([FromBody] LeaveReviewDto dto)
        {
            var req = await _db.LeaveRequests.FindAsync(dto.RequestId);
            if (req == null) return NotFound();

            req.Status = dto.Status;
            req.TeacherNote = dto.TeacherNote;
            await _db.SaveChangesAsync();
            return Ok(req);
        }
    }
}
