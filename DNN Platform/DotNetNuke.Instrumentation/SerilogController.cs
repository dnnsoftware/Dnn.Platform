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
    /// The Serilog logger provider instance.
    /// </summary>
    private static SerilogLoggerProvider provider;

    /// <summary>
    /// Gets the Serilog logger provider instance.
    /// </summary>
    internal static SerilogLoggerProvider Provider
    {
        get
        {
            return provider;
        }
    }

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

        builder.AddProvider(provider);
        builder.AddFilter<SerilogLoggerProvider>(null, LogLevel.Trace);

        return builder;
    }

    /// <summary>
    /// Sets up Serilog using the config file ~/Serilog.config.
    /// </summary>
    /// <param name="applicationMapPath">Path to the root of the DNN installation.</param>
    internal static void AddSerilog(string applicationMapPath)
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
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .Build());
        }
        else
        {
            config = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(applicationMapPath, "Portals\\_default\\Logs\\log.resources"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture)
                .MinimumLevel.Error();
        }

        Log.Logger = config.CreateLogger();
        provider = new SerilogLoggerProvider(Log.Logger);
    }
}
