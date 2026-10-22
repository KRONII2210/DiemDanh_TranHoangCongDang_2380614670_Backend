using Microsoft.AspNetCore.Identity;

namespace DiemDanh_TranHoangCongDang_2380614670_API.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Lecturer = "Lecturer";
        public const string Student = "Student";
        public static readonly string[] All = { Admin, Lecturer, Student };
    }

    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string? Code { get; set; } // MSSV: 2380614670
        public string? FaceTemplate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Room { get; set; } = "E1-02.05";
        public int TotalSessions { get; set; } = 15;
        public int MaxAbsentPercent { get; set; } = 20;
        public string? LecturerId { get; set; }
        public AppUser? Lecturer { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<ClassSession> Sessions { get; set; } = new();
    }

    public class Enrollment
    {
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string StudentId { get; set; } = "";
        public AppUser? Student { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }

    public class ClassSession
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public bool IsOpen { get; set; } = true;
        public string Secret { get; set; } = Guid.NewGuid().ToString("N");
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int RadiusMeters { get; set; } = 100;
        public List<Attendance> Attendances { get; set; } = new();
    }

    public class Attendance
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public ClassSession? Session { get; set; }
        public string StudentId { get; set; } = "";
        public AppUser? Student { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceMeters { get; set; }
        public string Method { get; set; } = "QR";
        public string Status { get; set; } = "PRESENT";
        public string? Note { get; set; }
    }

    public class LeaveRequest
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public ClassSession? Session { get; set; }
        public string StudentId { get; set; } = "";
        public AppUser? Student { get; set; }
        public string Reason { get; set; } = "";
        public string? ProofUrl { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
        public string? TeacherNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
