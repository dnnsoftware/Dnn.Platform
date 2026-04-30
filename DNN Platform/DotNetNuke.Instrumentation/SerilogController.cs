// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation;

using System;
using System.Globalization;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

/// <summary>
/// Controller for Serilog functions.
/// </summary>
internal static class SerilogController
{
    /// <summary>
    /// Gets the Serilog logger provider instance.
    /// </summary>
    internal static ILoggerProvider Provider { get; } = CreateSerilogLoggerProvider();

    /// <summary>
    /// Adds DNN Serilog configuration to the logging builder.
    /// </summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <returns>The configured logging builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static ILoggingBuilder AddDnnSerilog(this ILoggingBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddProvider(Provider);
        builder.AddFilter<SerilogLoggerProvider>(null, LogLevel.Trace);

        return builder;
    }

    /// <summary>
    /// Helper method to trigger the static initialization of this class which triggers assignment of the singleton (static property) <see cref="Provider"/> which in turn calls <see cref="InitializeSerilog(string)"/> and initializes the singleton <see cref="Log.Logger"/>.
    /// </summary>
    internal static void Initialize()
    {
    }

    private static SerilogLoggerProvider CreateSerilogLoggerProvider()
    {
        // initialize Serilog
        var applicationMapPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
        InitializeSerilog(applicationMapPath);
        return new SerilogLoggerProvider(Log.Logger);
    }

    /// <summary>
    /// Sets up Serilog using the config file ~/Serilog.config.
    /// </summary>
    /// <param name="applicationMapPath">Path to the root of the DNN installation.</param>
    private static void InitializeSerilog(string applicationMapPath)
    {
        Environment.SetEnvironmentVariable("BASEDIR", applicationMapPath);
        var configFile = Path.Combine(applicationMapPath, "Serilog.config");

        if (!File.Exists(configFile))
        {
            var defaultConfigFile = Path.Combine(applicationMapPath, "Config", "Serilog.default.config");
            if (File.Exists(defaultConfigFile))
            {
                File.Copy(defaultConfigFile, configFile);
            }
        }

        LoggerConfiguration config;
        if (File.Exists(configFile))
        {
            config = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .Build());
        }
        else
        {
            config = new LoggerConfiguration()
                .WriteTo.File(
                    Path.Combine(applicationMapPath, "Portals\\_default\\Logs\\log.resources"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture)
                .MinimumLevel.Error();
        }

        Log.Logger = config.CreateLogger();
    }
}
