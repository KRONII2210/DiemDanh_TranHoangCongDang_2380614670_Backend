namespace DiemDanh_TranHoangCongDang_2380614670_API.Dtos
{
    public class RegisterDto
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Code { get; set; } // MSSV
    }

    public class LoginDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class VerifyOtpDto
    {
        public string Username { get; set; } = "";
        public string Code { get; set; } = "";
    }

    public class FaceLoginDto
    {
        public string Username { get; set; } = "";
        public List<double> FaceVector { get; set; } = new();
    }

    public class CheckInDto
    {
        public int SessionId { get; set; }
        public string Code { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CourseCreateDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Room { get; set; }
        public int TotalSessions { get; set; } = 15;
        public int MaxAbsentPercent { get; set; } = 20;
    }

    public class SessionCreateDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int RadiusMeters { get; set; } = 100;
    }

    public class LeaveRequestDto
    {
        public int SessionId { get; set; }
        public string Reason { get; set; } = "";
        public string? ProofUrl { get; set; }
    }

    public class LeaveReviewDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; } = "APPROVED"; // APPROVED, REJECTED
        public string? TeacherNote { get; set; }
    }

    public class QrUpdateDto
    {
        public string Token { get; set; } = "";
    }
}
