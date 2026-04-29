// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using System;

using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

/// <summary>Provides centralized logging functionality for DNN Platform using Serilog.</summary>
public class DnnLoggingController
{
    private static ILoggerFactory loggerFactory;

    /// <summary>Initializes the Serilog logger factory.</summary>
    /// <remarks>
    /// This method can be called to explicitly initialize the logging system.
    /// If not called explicitly, the logger factory will be initialized automatically
    /// when the first logger is requested.
    /// </remarks>
    public static void InitializeLoggerFactory()
    {
        GetSerilogLoggerFactory();
    }

    /// <summary>Gets a strongly-typed logger instance for the specified type.</summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>An <see cref="ILogger{T}"/> instance configured with Serilog.</returns>
    /// <remarks>
    /// If Serilog has not been initialized, this method will automatically initialize it
    /// using the application's host path before creating the logger.
    /// </remarks>
    public static ILogger<T> GetLogger<T>()
    {
        return GetSerilogLoggerFactory().CreateLogger<T>();
    }

    /// <summary>Gets a strongly-typed logger instance for the specified type.</summary>
    /// <param name="type">The type for which to create the logger.</param>
    /// <returns>An <see cref="ILogger{T}"/> instance configured with Serilog.</returns>
    /// <remarks>
    /// If Serilog has not been initialized, this method will automatically initialize it
    /// using the application's host path before creating the logger.
    /// </remarks>
    public static Microsoft.Extensions.Logging.ILogger GetLogger(Type type)
    {
        return GetSerilogLoggerFactory().CreateLogger(type);
    }

    /// <summary>Gets a strongly-typed logger instance for the specified type.</summary>
    /// <param name="categoryName">The category name to associated with the logger.</param>
    /// <returns>An <see cref="ILogger{T}"/> instance configured with Serilog.</returns>
    /// <remarks>
    /// If Serilog has not been initialized, this method will automatically initialize it
    /// using the application's host path before creating the logger.
    /// </remarks>
    public static Microsoft.Extensions.Logging.ILogger GetLogger(string categoryName)
    {
        return GetSerilogLoggerFactory().CreateLogger(categoryName);
    }

    private static ILoggerFactory GetSerilogLoggerFactory()
    {
        if (loggerFactory == null)
        {
            // initialize Serilog
            var applicationMapPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
            SerilogController.AddSerilog(applicationMapPath);
            loggerFactory = new SerilogLoggerFactory(Log.Logger);
        }

        return loggerFactory;
    }
}
