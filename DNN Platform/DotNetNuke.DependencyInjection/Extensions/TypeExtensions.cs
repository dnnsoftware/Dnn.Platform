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
        // There is no logging in this file by design as
        // it would create a dependency on the Logging library
        // and this library can't have any dependencies on other
        // DNN Libraries.

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
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static Type[] SafeGetTypes(this Assembly assembly, ILog logger = null)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
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
                    logger.Error($"Unable to configure services for {assembly.FullName}, see exception for details", ex);

                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        logger.Error($"LoaderException for {assembly.FullName}, details", loaderEx);
                    }
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
