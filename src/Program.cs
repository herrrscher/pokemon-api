using Microsoft.OpenApi.Models;
using poke_db.Services;
using poke_db.Security;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var linuxTheme = new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
{
    [ConsoleThemeStyle.Text] = "\x1b[37m",             // White text
    [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",    // Dark gray
    [ConsoleThemeStyle.TertiaryText] = "\x1b[37m",     // White
    [ConsoleThemeStyle.Invalid] = "\x1b[31m",          // Red
    [ConsoleThemeStyle.Null] = "\x1b[95m",             // Magenta
    [ConsoleThemeStyle.Name] = "\x1b[93m",             // Yellow
    [ConsoleThemeStyle.String] = "\x1b[96m",           // Cyan
    [ConsoleThemeStyle.Number] = "\x1b[95m",           // Magenta
    [ConsoleThemeStyle.Boolean] = "\x1b[95m",          // Magenta
    [ConsoleThemeStyle.Scalar] = "\x1b[95m",           // Magenta
    [ConsoleThemeStyle.LevelVerbose] = "\x1b[90m",     // Dark gray
    [ConsoleThemeStyle.LevelDebug] = "\x1b[36m",       // Cyan (like Fedora debug)
    [ConsoleThemeStyle.LevelInformation] = "\x1b[32m", // Green (like Fedora info)
    [ConsoleThemeStyle.LevelWarning] = "\x1b[33m",     // Yellow (like Fedora warning)
    [ConsoleThemeStyle.LevelError] = "\x1b[31m",       // Red (like Fedora error)
    [ConsoleThemeStyle.LevelFatal] = "\x1b[91m"        // Bright red
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: linuxTheme)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAuthenticationFilter>();
});

builder.Services.AddScoped<ApiKeyAuthenticationFilter>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pokémon Database API",
        Version = "v1.0.0",
        Description = "A comprehensive RESTful API for Pokémon data management and retrieval.",
        Contact = new OpenApiContact
        {
            Name = "Pokémon DB Team",
            Email = "api-support@pokemon-db.com"
        }
    });
});

var realtimeDbUrl = builder.Configuration["Firebase:RealtimeDatabaseUrl"];
if (string.IsNullOrEmpty(realtimeDbUrl))
{
    throw new InvalidOperationException("Realtime Database URL is not configured in appsettings.json");
}
builder.Services.AddScoped<IPokemonService>(sp => new RealtimePokemonService(realtimeDbUrl));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pokémon Database API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors();

app.UseMiddleware<RateLimitingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

var urls = app.Urls.Count > 0 ? app.Urls : new[] { "http://localhost:5000" };
foreach (var url in urls)
{
    Console.WriteLine($"API docs: {url}/");
    Console.WriteLine($"Health: {url}/health");
}

app.Run();
