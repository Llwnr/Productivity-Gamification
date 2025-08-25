using System.Text;
using Gamification.Core.Interfaces;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Gamification.Infrastructure.Interfaces;
using Gamification.Infrastructure.Services;
using Gamification.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add services to the container.
builder.Services.AddDbContextPool<ProductivityDbContext>(option =>
    option.UseNpgsql(builder.Configuration.GetConnectionString("ProductivityDb"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<GoogleApi>();

builder.Services.AddScoped<IScoreProcessingService, ScoreProcessingService>();
builder.Services.AddScoped<ISiteAnalysisService, SiteAnalysisService>();
builder.Services.AddScoped<IInactivityRecordingService, InactivityRecordingService>();

builder.Services.AddSingleton<AnalysisQueryManager>();
builder.Services.AddSingleton<IAnalysisQueryManager>(provider => 
    provider.GetRequiredService<AnalysisQueryManager>());
builder.Services.AddHostedService(provider => 
    provider.GetRequiredService<AnalysisQueryManager>());

builder.Services.AddSingleton<IContentAnalysisFilter, ContentAnalysisFilter>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters{
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:7131",
            ValidAudience = "https://localhost:7131",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtKeys:SymmetricKey")))
        };
        options.Events = new JwtBearerEvents{
            OnMessageReceived = context => {
                context.Token = context.Request.Cookies["authToken"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options => {
    options.AddPolicy("ExtensionPolicy", builder => {
        builder.WithOrigins("chrome-extension://caigbhogbomcfecinondmiddlbgjmgce")
            .AllowAnyHeader()
            .AllowAnyMethod() // GET, POST, etc.
            .AllowCredentials(); // ESSENTIAL: Allows the browser to send HttpOnly cookies cross-origin
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// using (var scope = app.Services.CreateScope()){
//     var services = scope.ServiceProvider;
//     Console.WriteLine("Eagerly loading the ContentAnalysisFilter service...");
//     services.GetRequiredService<IContentAnalysisFilter>();
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()){
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()){
    var services = scope.ServiceProvider;
    Console.WriteLine("Loading score processing service...");
    services.GetRequiredService<IScoreProcessingService>();
}

app.Run();

