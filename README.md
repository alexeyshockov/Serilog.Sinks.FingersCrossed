# Serilog.Sinks.FingersCrossed

[![NuGet Serilog.Sinks.FingersCrossed](https://img.shields.io/nuget/dt/Serilog.Sinks.FingersCrossed)](https://www.nuget.org/packages/Serilog.Sinks.FingersCrossed/)
[![Code coverage](https://img.shields.io/sonar/coverage/alexeyshockov_Serilog.Sinks.FingersCrossed?server=https%3A%2F%2Fsonarcloud.io)](https://sonarcloud.io/project/overview?id=alexeyshockov_Serilog.Sinks.FingersCrossed)

Buffered sink wrapper for Serilog.

## Getting started

Install from [NuGet](https://nuget.org/packages/serilog.sinks.fingerscrossed):

`dotnet` CLI:
```shell
dotnet add package Serilog.Sinks.FingersCrossed
```

PowerShell:
```PowerShell
Install-Package Serilog.Sinks.FingersCrossed
```

Assuming you have already installed the target sink, such as the file sink, move the wrapped sink's configuration within
a `WriteTo.FingersCrossed()` statement:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.FingersCrossed(a => a.Console())
    // Other logger configuration
    .CreateLogger()

using (LogBuffer.BeginScope())
    Log.Information("This will be dropped");

using (LogBuffer.BeginScope())
{
    Log.Information("This will be written to the console");
    Log.Error("Because there is an error in the scope");
}

Log.CloseAndFlush();
```

## About this sink

The sink was heavily inspired by [PHP's Monolog `fingers_crossed` handler](https://seldaek.github.io/monolog/doc/02-handlers-formatters-processors.html).
