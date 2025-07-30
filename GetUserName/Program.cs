using Microsoft.AspNetCore.Server.IISIntegration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Serve Angular static files
app.UseDefaultFiles(); // Serves index.html by default
app.UseStaticFiles();  // Serves js/css/assets from wwwroot

// API endpoint for Windows username
app.MapGet("/api/username", (HttpContext context) =>
{
    var username = context.User.Identity?.Name;
    return Results.Ok(new { username });
}).RequireAuthorization();

// Fallback to Angular index.html for client-side routes
app.MapFallback(async context =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(filePath);
});

app.Run();
