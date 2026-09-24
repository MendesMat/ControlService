using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Every feature registers its endpoints under /api/v1 (ADR-0003), for example: api.MapUserEndpoints();
var api = app.MapGroup("/api/v1");

app.MapDefaultEndpoints();

app.Run();
