using System.Runtime.Versioning;
using AccountingServer.DBAccess;
using AccountingServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//builder.Logging.ClearProviders();

// Conditionally add EventLog logging only on Windows platforms
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}

builder.Services.AddSingleton<DynamicsDBContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();
app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapInvoicingEndpoints();
app.MapGeneralEndpoints();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.Run();