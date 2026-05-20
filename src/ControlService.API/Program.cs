using ControlService.API.Exceptions;
using Prometheus;
using Scalar.AspNetCore;
using ControlService.Commercial.Application;
using ControlService.Commercial.Infrastructure;
using ControlService.Management.Application;
using ControlService.Management.Infrastructure;
using ControlService.Operational.Application;
using ControlService.Operational.Infrastructure;
using ControlService.Financial.Application;
using ControlService.Financial.Infrastructure;
using ControlService.Reporting.Application;
using ControlService.Reporting.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Exception Handling ─────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── Application Layer ──────────────────────────────────────────────────────────
builder.Services.AddCommercialApplication();
builder.Services.AddManagementApplication();
builder.Services.AddOperationalApplication();
builder.Services.AddFinancialApplication();
builder.Services.AddReportingApplication();

// ── Infrastructure Layer ───────────────────────────────────────────────────────
builder.Services.AddCommercialInfrastructure(builder.Configuration);
builder.Services.AddManagementInfrastructure(builder.Configuration);
builder.Services.AddOperationalInfrastructure(builder.Configuration);
builder.Services.AddFinancialInfrastructure(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);

// ── Scalar / OpenAPI (Scalar) ─────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseHttpMetrics();
app.MapMetrics();

app.UseExceptionHandler();

// ── Health Check (Liveness Probe) ──────────────────────────────────────────────
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }))
   .WithTags("Health")
   .ExcludeFromDescription();

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
