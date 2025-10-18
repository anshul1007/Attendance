using AttendanceAPI.Data;
using AttendanceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(defaultConn))
{
    throw new InvalidOperationException("DefaultConnection is not configured");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Improved heuristic: detect SQL Server by common SQL Server keywords, otherwise prefer Npgsql
    var lower = defaultConn.ToLowerInvariant();

    bool looksLikeSqlServer = lower.Contains("initial catalog")
                            || lower.Contains("data source")
                            || lower.Contains("server=")
                            || lower.Contains("tcp:")
                            || (lower.Contains("database=") && lower.Contains("user id="));

    bool looksLikePostgres = lower.Contains("host=") || lower.Contains("port=") || lower.Contains("postgres");

    // Mask password for logs
    string maskedConn = defaultConn;
    try
    {
        var pwdIdx = maskedConn.ToLowerInvariant().IndexOf("password=");
        if (pwdIdx >= 0)
        {
            var end = maskedConn.IndexOf(';', pwdIdx);
            if (end == -1) end = maskedConn.Length;
            maskedConn = maskedConn.Remove(pwdIdx + "password=".Length, Math.Min(8, end - (pwdIdx + "password=".Length))).Insert(pwdIdx + "password=".Length, "********");
        }
    }
    catch { /* ignore masking errors */ }

    if (looksLikeSqlServer && !looksLikePostgres)
    {
        Console.WriteLine($"[Startup] Connection string appears to be SQL Server. Using SqlServer provider. Conn: {maskedConn}");
        options.UseSqlServer(defaultConn);
    }
    else if (looksLikePostgres && !looksLikeSqlServer)
    {
        Console.WriteLine($"[Startup] Connection string appears to be Postgres. Using Npgsql provider. Conn: {maskedConn}");
        options.UseNpgsql(defaultConn);
    }
    else
    {
        // Mixed/unknown: prefer SqlServer for Azure SQL style or fallback to SqlServer
        Console.WriteLine($"[Startup] Connection string ambiguous. Preferring SqlServer provider. Conn: {maskedConn}");
        options.UseSqlServer(defaultConn);
    }
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database
await DatabaseSeeder.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
