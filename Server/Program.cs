using AccountingServer.DBAccess;
using AccountingServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.Run();