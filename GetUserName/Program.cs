using Microsoft.AspNetCore.Server.IISIntegration;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel().UseIISIntegration();
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/username", (HttpContext context) =>
{
    var username = context.User.Identity?.Name; // e.g., DOMAIN\username
    return Results.Ok(new { username });
})
.RequireAuthorization(); // Only allow authenticated users

app.Run();
