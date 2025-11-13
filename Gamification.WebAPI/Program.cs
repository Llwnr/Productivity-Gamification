using System.Text;
using System.Threading.Channels;
using Gamification.Core.GameModels;
using Gamification.Core.Interfaces;
using Gamification.Infrastructure.Events;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Gamification.Infrastructure.Interfaces;
using Gamification.Infrastructure.Services;
using Gamification.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add services to the container.
builder.Services.AddDbContextPool<ProductivityDbContext>(option =>
    option.UseNpgsql(builder.Configuration.GetConnectionString("ProductivityDb"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<GoogleApi>();

builder.Services.AddScoped<ISiteAnalysisService, SiteAnalysisService>();
builder.Services.AddScoped<IInactivityRecordingService, InactivityRecordingService>();
builder.Services.AddScoped<IActivityRecorder, ActivityRecorder>();
builder.Services.AddScoped<IActivityProcessingService, ActivityProcessingService>();
builder.Services.AddScoped<IStreakManagementService, ActivityProcessingService>();

builder.Services.AddSingleton<AnalysisQueryManager>();
builder.Services.AddSingleton<IAnalysisQueryManager>(
    sp => sp.GetRequiredService<AnalysisQueryManager>());
builder.Services.AddHostedService<AnalysisQueryManager>(
    sp => sp.GetRequiredService<AnalysisQueryManager>());

builder.Services.AddHostedService<ScheduledProcessingService>();

builder.Services.AddSingleton<IContentAnalysisFilter, ContentAnalysisFilter>();
builder.Services.AddSingleton<Channel<GameEvent>>(
    _ => Channel.CreateUnbounded<GameEvent>());

builder.Services.AddHostedService<AchievementManager>();

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
        builder.WithOrigins("chrome-extension://caigbhogbomcfecinondmiddlbgjmgce","http://localhost:4200")
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

// if (app.Environment.IsDevelopment()){
//     using (var scope = app.Services.CreateScope()){
//         var services = scope.ServiceProvider;
//         var dbContext = services.GetRequiredService<ProductivityDbContext>();
//         
//         // Optional: Ensure the database is created and migrations are applied
//         // await dbContext.Database.MigrateAsync();
//
//         var seeder = new DataSeeder(dbContext);
//         // Call the new method to add data up to the target
//         await seeder.SeedAdditionalDataAsync(10000); 
//     }
// }

using (var scope = app.Services.CreateScope()){
    var services = scope.ServiceProvider;
    // Console.WriteLine("Eagerly loading the ContentAnalysisFilter service...");
    // services.GetRequiredService<IContentAnalysisFilter>();

    var dbContext = services.GetRequiredService<ProductivityDbContext>();
    try{
        if (!dbContext.Achievements.Any()){
            Console.WriteLine("Adding achievements");
            dbContext.Achievements.AddRange(
                AchievementDefinition.GetAchievementDefinitions());
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex){
        Console.WriteLine("Exception when adding achievement definitions: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()){
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ExtensionPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

