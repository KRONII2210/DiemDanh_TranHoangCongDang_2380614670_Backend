using DiemDanh_TranHoangCongDang_2380614670_API.Data;
using DiemDanh_TranHoangCongDang_2380614670_API.Models;
using DiemDanh_TranHoangCongDang_2380614670_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("JWTKey");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwt["ValidIssuer"],
        ValidAudience = jwt["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!))
    };
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DiemDanh_TranHoangCongDang_2380614670_API",
        Version = "v1",
        Description = "Web API Điểm danh sinh viên - Nhóm TSV - Trần Hoàng Công Đăng (MSSV: 2380614670)"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiemDanh_TranHoangCongDang_2380614670_API v1");
});

app.UseCors("AllowAll");

// Phục vụ giao diện Web Điểm danh trực tiếp từ root (bỏ qua bước trung gian Swagger)
app.UseDefaultFiles();
app.UseStaticFiles();

var flutterWebPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "diemdanh_tranhoangcongdang_2380614670", "build", "web"));
if (Directory.Exists(flutterWebPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(flutterWebPath),
        RequestPath = ""
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Màn hình chiếu phát mã QR động của Giảng viên (Truy cập trên máy tính để điện thoại quét mã)
app.MapGet("/qr", () => Results.Content(QrWebPage.GetHtml(), "text/html"));

// SPA Fallback về index.html của Web App Điểm danh
app.MapFallbackToFile("index.html");

// Nạp dữ liệu mẫu ban đầu
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
