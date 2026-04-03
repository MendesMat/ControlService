using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ControlService.API.Exceptions;
using ControlService.Application.Behaviors;
using ControlService.Infrastructure.Data;
using ControlService.Infrastructure.Data.Repositories.Commercial;
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

// ── Database ───────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ControlServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

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
