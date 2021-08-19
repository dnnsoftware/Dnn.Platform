// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Instrumentation;

    /// <summary>
    /// <see cref="Type"/> specific extensions to be used
    /// in Dependency Injection invocations.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Safely Get all Types from the assembly. If there
        /// is an error while retrieving the types it will
        /// return an empty array of <see cref="Type"/>.
        /// </summary>
        /// <param name="assembly">The assembly to retrieve all types from.</param>
        /// <returns>An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.</returns>
        /// <remarks>This is obsolete because logging is not added. Please use the SafeGetTypes with the ILog parameter.</remarks>
        [Obsolete("Deprecated in DotNetNuke 9.9.0. Please use the SafeGetTypes overload with the ILog parameter. Scheduled removal in v11.0.0.")]
        public static Type[] SafeGetTypes(this Assembly assembly)
        {
            return assembly.SafeGetTypes(null);
        }

        /// <summary>
        /// Safely Get all Types from the assembly. If there
        /// is an error while retrieving the types it will
        /// return an empty array of <see cref="Type"/>.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to retrieve all types from.
        /// </param>
        /// <param name="logger">
        /// A optional <see cref="ILog"/> object. This will log any messages from the exception catching to the logs as an Error.
        /// </param>
        /// <returns>
        /// An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.
        /// </returns>
        public static Type[] SafeGetTypes(this Assembly assembly, ILog logger = null)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (logger != null)
                {
                    // The loaderexceptions will repeat. Need to get distinct messages here.
                    string distinctLoaderExceptions = string.Join(
                        Environment.NewLine,
                        ex.LoaderExceptions.Select(i => i.Message).Distinct().Select(i => $"- LoaderException: {i}"));

                    logger.Error($"Unable to configure services for {assembly.FullName}, see exception for details {Environment.NewLine}{distinctLoaderExceptions}", ex);
                }

                // Ensure that Dnn obtains all types that were loaded, ignoring the failure(s)
                types = ex.Types.Where(x => x != null).ToArray<Type>();
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error($"Unable to configure services for {assembly.FullName}, see exception for details", ex);
                }

                types = new Type[0];
            }

            return types;
        }
    }
}
