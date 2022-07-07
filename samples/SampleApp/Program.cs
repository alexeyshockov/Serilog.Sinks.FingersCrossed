using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.FingersCrossed;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, lc) => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.FingersCrossed(lsc => lsc.Console())
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    using var logBuffer = LogBuffer.BeginScope();
    try
    {
        await next(context);

        if (context.Response.StatusCode >= 400)
            logBuffer.Flush();
    }
    catch (Exception)
    {
        logBuffer.Flush();

        throw;
    }
});
if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();
app.UseAuthorization();

app.MapGet("/hello", ([FromServices] ILogger<WebApplication> logger) =>
    logger.LogInformation("This goes nowhere"));
app.MapGet("/error", ([FromServices] ILogger<WebApplication> logger) =>
    logger.LogError("Arbitrary error"));

app.Run();
