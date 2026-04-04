using ControlService.API.Exceptions;
using ControlService.Application;
using ControlService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Exception Handling ─────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── Application Layer ──────────────────────────────────────────────────────────
builder.Services.AddApplication();

// ── Infrastructure Layer ───────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Scalar / OpenAPI (Scalar) ─────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("ControlService API")
               .WithDownloadButton(true)
               .WithTheme(ScalarTheme.Moon);
    });

    app.MapGet("/", () => Results.Redirect("/docs"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
