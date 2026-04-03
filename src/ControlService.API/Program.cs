using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ControlService.API.Exceptions;
using ControlService.Application.Behaviors;
using ControlService.Infrastructure;
using ControlService.Domain.Commercial.Customers;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Exception Handling ─────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── MediatR ────────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ControlService.Application.Behaviors.ValidationPipelineBehavior<,>).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
});

// ── FluentValidation ───────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(typeof(ControlService.Application.Behaviors.ValidationPipelineBehavior<,>).Assembly);

// ── Infrastructure ─────────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
