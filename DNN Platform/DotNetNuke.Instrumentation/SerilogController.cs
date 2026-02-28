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
    /// Sets up Serilog using the config file ~/Config/Serilog.config.
    /// </summary>
    /// <param name="hostMapPath">Path to the root of the DNN installation.</param>
    internal static void AddSerilog(string hostMapPath)
    {
        var configFile = hostMapPath + "\\Config\\Serilog.config";
        var config = new LoggerConfiguration()
            .WriteTo.File(hostMapPath + "\\Portals\\_default\\Logs\\log.txt", rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture)
            .MinimumLevel.Debug();
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
