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
    /// <param name="hostMapPath">Path to the root of the DNN installation.</param>
    internal static void AddSerilog(string hostMapPath)
    {
        var configFile = Path.Combine(hostMapPath, "Serilog.config");
        var config = new LoggerConfiguration()
            .WriteTo.File(
                Path.Combine(hostMapPath, "Portals\\_default\\Logs\\log.txt"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                formatProvider: CultureInfo.InvariantCulture)
            .MinimumLevel.Error();

        if (!File.Exists(configFile))
        {
            var defaultConfigFile = Path.Combine(hostMapPath, "Config", "Serilog.default.config");
            if (File.Exists(defaultConfigFile))
            {
                File.Copy(defaultConfigFile, configFile);
            }
        }

        if (File.Exists(configFile))
        {
            config = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .Build());
        }

        Log.Logger = config.CreateLogger();
    }
}
