using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using mperformancepower.Api.Data;
using mperformancepower.Api.Hubs;
using mperformancepower.Api.Middleware;
using mperformancepower.Api.Services;
using mperformancepower.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 0))));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Allow JWT from query string for SignalR
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/api/hubs"))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints: 10 attempts per minute per IP
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// CORS
builder.Services.AddCors(o => o.AddPolicy("AppPolicy", p =>
    p.WithOrigins(
        "http://localhost",
        "http://localhost:8080",
        "http://localhost:5173",
        "http://localhost:5174",
        "https://mperformancepower.com",
        "https://www.mperformancepower.com")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// SignalR
builder.Services.AddSignalR();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IMailService, MailService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AppPolicy");
app.UseRateLimiter();
app.UseStaticFiles(); // serves wwwroot/uploads
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/api/hubs/notifications");
app.MapHub<ChatHub>("/api/hubs/chat");

// Create schema and seed on startup (no migrations folder — EnsureCreated manages schema)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await DbSeeder.SeedAsync(db);
}

app.Run();
