// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation;

using System;

using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// The startup extensions to add Serilog to the project.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Add Serilog to the project.
    /// </summary>
    /// <param name="services">The IServiceCollection.</param>
    /// <param name="applicationMapPath">The path to the root of the DotNetNuke website. This is needed to find the correct directory to write the log files to.</param>
    public static void AddSerilog(this IServiceCollection services, string applicationMapPath)
    {
        Environment.SetEnvironmentVariable("BASEDIR", applicationMapPath);
        SerilogController.AddSerilog(applicationMapPath);
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(null, true));
    }
}
