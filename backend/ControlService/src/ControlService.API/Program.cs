using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Infrastructure;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Unhandled exceptions become the generic 500 of API-12; ErrorResults.ToProblem handles Result failures.
builder.Services.AddExceptionHandler<UnexpectedErrorExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ICurrentUser and TimeProvider.System are registered here (T12) because AuditFieldsInterceptor
// (registered by AddInfrastructure) depends on both.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton(TimeProvider.System);

builder.AddInfrastructure();
builder.Services.AddAuthFeature(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Migrations run automatically only here; MigrateAsync also runs the seeder (D2).
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Every feature registers its endpoints under /api/v1, for example: api.MapUserEndpoints();
var api = app.MapApiV1();
api.MapAuthEndpoints();

app.MapDefaultEndpoints();

app.Run();
