// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation;

using System.Globalization;
using System.IO;

using Microsoft.Extensions.Configuration;
using Serilog;

/// <summary>
/// Controller for Serilog functions.
/// </summary>
internal sealed class SerilogController
{
    /// <summary>
    /// Sets up Serilog using the config file ~/Serilog.config.
    /// </summary>
    /// <param name="applicationMapPath">Path to the root of the DNN installation.</param>
    internal static void AddSerilog(string applicationMapPath)
    {
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
                    Path.Combine(applicationMapPath, "Portals\\_default\\Logs\\log.log.resources"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture)
                .MinimumLevel.Error();
        }

        Log.Logger = config.CreateLogger();
    }
}
