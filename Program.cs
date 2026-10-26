using System.Threading.RateLimiting;
using ExamApp.Api.Application.Contracts;
using ExamApp.Api.Application.Services;
using ExamApp.Api.Infrastructure.Data;
using ExamApp.Api.Infrastructure.Data.Seed;
using ExamApp.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ExamDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();

builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));

static RateLimitPartition<string> PerIp(HttpContext context, int permitLimit) =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("exam-submit", context => PerIp(context, permitLimit: 5));
    options.AddPolicy("questions-read", context => PerIp(context, permitLimit: 30));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ExamDbContext>();

    await context.Database.MigrateAsync();

    var seedPath = Path.Combine(app.Environment.ContentRootPath, "Data", "questions.json");
    await QuestionSeeder.SeedAsync(context, seedPath, logger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
