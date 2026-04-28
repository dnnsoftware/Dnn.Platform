// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;

    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Extensions.Logging;

    /// <summary>Provides centralized logging functionality for DNN Platform using Serilog.</summary>
    public class DnnLoggingController
    {
        private static ILoggerFactory loggerFactory;

        /// <summary>Gets a strongly-typed logger instance for the specified type.</summary>
        /// <typeparam name="T">The type for which to create the logger.</typeparam>
        /// <returns>An <see cref="ILogger{T}"/> instance configured with Serilog.</returns>
        /// <remarks>
        /// If Serilog has not been initialized, this method will automatically initialize it
        /// using the application's host path before creating the logger.
        /// </remarks>
        public static ILogger<T> GetLogger<T>()
        {
            if (Log.Logger == null)
            {
                // initialize Serilog
                var applicationMapPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                SerilogController.AddSerilog(applicationMapPath);
            }

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
            if (Log.Logger == null)
            {
                // initialize Serilog
                var applicationMapPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                SerilogController.AddSerilog(applicationMapPath);
            }

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
            if (Log.Logger == null)
            {
                // initialize Serilog
                var applicationMapPath = System.Web.Hosting.HostingEnvironment.MapPath("~");
                SerilogController.AddSerilog(applicationMapPath);
            }

            return GetSerilogLoggerFactory().CreateLogger(categoryName);
        }

        /// <summary>Adds a property to the Serilog log context.</summary>
        /// <param name="key">The property key/name to add to the log context.</param>
        /// <param name="value">The value to associate with the property.</param>
        /// <remarks>
        /// Properties added to the log context will be included in all subsequent log events
        /// until the context is disposed or cleared. The property is added using Serilog's
        /// <see cref="Serilog.Context.LogContext.PushProperty(string, object, bool)"/> method.
        /// </remarks>
        public static void AddToLogContext(string key, object value)
        {
            Serilog.Context.LogContext.PushProperty(key, value);
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
}
