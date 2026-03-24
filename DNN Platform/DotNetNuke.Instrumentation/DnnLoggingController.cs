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

            return new SerilogLoggerFactory(Log.Logger).CreateLogger<T>();
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

            return new SerilogLoggerFactory(Log.Logger).CreateLogger(type);
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

            return new SerilogLoggerFactory(Log.Logger).CreateLogger(categoryName);
        }
    }
}
