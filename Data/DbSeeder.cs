using DiemDanh_TranHoangCongDang_2380614670_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiemDanh_TranHoangCongDang_2380614670_API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Tự tạo Database nếu chưa có
            await db.Database.EnsureCreatedAsync();

            foreach (var role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            async Task<AppUser> EnsureUser(string username, string email, string password, string fullName, string? code, string role)
            {
                var u = await userManager.FindByNameAsync(username);
                if (u != null) return u;
                u = new AppUser
                {
                    UserName = username,
                    Email = email,
                    FullName = fullName,
                    Code = code,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(u, password);
                await userManager.AddToRoleAsync(u, role);
                return u;
            }

            // Tạo các tài khoản mẫu chuẩn
            await EnsureUser("admin", "admin@tsv.edu.vn", "Admin@123", "Quản Trị Viên TSV", null, Roles.Admin);
            var gv = await EnsureUser("gv_khoa", "gv_khoa@tsv.edu.vn", "Gv@12345", "GV. Nguyễn Văn Giảng", "GV001", Roles.Lecturer);
            
            // SINH VIÊN BẮT BUỘC: TRẦN HOÀNG CÔNG ĐĂNG - MSSV: 2380614670
            var svDang = await EnsureUser("sv_dang", "dang.thc@tsv.edu.vn", "Sv@12345", "Trần Hoàng Công Đăng", "2380614670", Roles.Student);
            var svAn = await EnsureUser("sv_an", "an.tt@tsv.edu.vn", "Sv@12345", "Trần Thị An", "2380000001", Roles.Student);
            var svBinh = await EnsureUser("sv_binh", "binh.lv@tsv.edu.vn", "Sv@12345", "Lê Văn Bình", "2380000002", Roles.Student);

            if (!await db.Courses.AnyAsync())
            {
                var ltdd = new Course
                {
                    Code = "LTDD01",
                    Name = "Lập trình di động (Nhóm TSV)",
                    Room = "E1-02.05",
                    TotalSessions = 15,
                    MaxAbsentPercent = 20,
                    LecturerId = gv.Id
                };
                var csdl = new Course
                {
                    Code = "CSDL02",
                    Name = "Cơ sở dữ liệu",
                    Room = "E2-03.01",
                    TotalSessions = 10,
                    MaxAbsentPercent = 20,
                    LecturerId = gv.Id
                };
                db.Courses.AddRange(ltdd, csdl);
                await db.SaveChangesAsync();

                foreach (var sv in new[] { svDang, svAn, svBinh })
                {
                    db.Enrollments.Add(new Enrollment { CourseId = ltdd.Id, StudentId = sv.Id });
                    db.Enrollments.Add(new Enrollment { CourseId = csdl.Id, StudentId = sv.Id });
                }

                var b1 = new ClassSession
                {
                    CourseId = ltdd.Id,
                    Title = "Buổi 1: Giới thiệu Flutter & Dart UI",
                    StartTime = DateTime.UtcNow.AddDays(-14),
                    EndTime = DateTime.UtcNow.AddDays(-14).AddHours(3),
                    IsOpen = false,
                    Secret = "SECRET_1"
                };
                var b2 = new ClassSession
                {
                    CourseId = ltdd.Id,
                    Title = "Buổi 2: State Management & CRUD",
                    StartTime = DateTime.UtcNow.AddDays(-7),
                    EndTime = DateTime.UtcNow.AddDays(-7).AddHours(3),
                    IsOpen = false,
                    Secret = "SECRET_2"
                };
                var b3 = new ClassSession
                {
                    CourseId = ltdd.Id,
                    Title = "Buổi 3: QR Code & GPS Geofencing (TSV)",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(3),
                    IsOpen = true,
                    Secret = "SECRET_3",
                    Latitude = 10.8221,
                    Longitude = 106.6882,
                    RadiusMeters = 100
                };
                db.ClassSessions.AddRange(b1, b2, b3);
                await db.SaveChangesAsync();

                db.Attendances.AddRange(
                    new Attendance { SessionId = b1.Id, StudentId = svDang.Id, Method = "QR", Status = "PRESENT", Note = "Đúng giờ" },
                    new Attendance { SessionId = b1.Id, StudentId = svAn.Id, Method = "QR", Status = "PRESENT", Note = "Đúng giờ" },
                    new Attendance { SessionId = b2.Id, StudentId = svDang.Id, Method = "Face", Status = "PRESENT", Note = "Khuôn mặt Face ID" }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
