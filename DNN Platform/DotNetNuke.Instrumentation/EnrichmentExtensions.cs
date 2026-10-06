// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using Serilog;
using Serilog.Configuration;

/// <summary>
/// Provides extension methods for enriching Serilog logger configuration.
/// </summary>
public static class EnrichmentExtensions
{
    /// <summary>
    /// Enriches the logger configuration with DNN system information.
    /// </summary>
    /// <param name="input">The logger enrichment configuration.</param>
    /// <returns>The logger configuration with DNN system information enrichment applied.</returns>
    public static LoggerConfiguration WithDnnSystemInfo(this LoggerEnrichmentConfiguration input)
    {
        return input.With<DnnSystemInfoEnricher>();
    }
}
