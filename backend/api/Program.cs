using System.Security.Claims;
using Coravel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Wallanoti.Api;
using Wallanoti.Api.Extension.DependencyInjection;
using Wallanoti.Api.Middlewares.Auth;
using Wallanoti.Api.ScheduledTasks;
using Wallanoti.Api.Telegram.Configurations;
using Wallanoti.Src.Notifications.Infrastructure.Notifications;
using Wallanoti.Src.Shared.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey =
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
        
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(WebNotificationHub.HubName))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IUserIdProvider, TelegramUserIdProvider>();

builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSchedulerTask();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(o =>
{
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

    var securityRequirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { securityScheme, [JwtBearerDefaults.AuthenticationScheme] }
    };

    o.AddSecurityRequirement(securityRequirement);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

//TODO configurar rate limiter
// builder.Services.AddRateLimiter(options =>
// {
//     options.AddFixedWindowLimiter("authRateLimiter", opt =>
//     {
//         opt.PermitLimit = 5;
//         opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//         opt.QueueLimit = 2;
//         opt.Window = TimeSpan.FromSeconds(1);
//     });
// });

builder.Services.AddScoped<UserContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<AlertSearcher>()
        .EveryTenSeconds()
        .PreventOverlapping("AlertSearcher");
});

app.MapHub<WebNotificationHub>(WebNotificationHub.HubName);

// MassTransit starts and manages RabbitMQ consumers automatically as an IHostedService.
// No manual consumer setup required.

await using var scope2 = app.Services.CreateAsyncScope();
var telegramBot = scope2.ServiceProvider.GetRequiredService<TelegramBot>();
await telegramBot.Start();

app.Run();


//TODO mover a otro sitio
namespace Wallanoti.Api
{
    internal class TelegramUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}