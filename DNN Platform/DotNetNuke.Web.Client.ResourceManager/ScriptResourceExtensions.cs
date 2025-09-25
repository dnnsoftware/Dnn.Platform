// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IScriptResource"/> to set various script resource properties in a fluent manner.
    /// </summary>
    public static class ScriptResourceExtensions
    {
        /// <summary>
        /// Sets the async attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the async attribute for.</param>
        /// <returns>The script resource with the async attribute set.</returns>
        public static IScriptResource SetAsync(this IScriptResource input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Async = true;
            return input;
        }

        /// <summary>
        /// Sets the defer attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the defer attribute for.</param>
        /// <returns>The script resource with the defer attribute set.</returns>
        public static IScriptResource SetDefer(this IScriptResource input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Defer = true;
            return input;
        }

        /// <summary>
        /// Sets the no module attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the no module attribute for.</param>
        /// <returns>The script resource with the no module attribute set.</returns>
        public static IScriptResource SetNoModule(this IScriptResource input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.NoModule = true;
            return input;
        }

        /// <summary>
        /// Sets the type attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the type attribute for.</param>
        /// <param name="type">The type attribute to set.</param>
        /// <returns>The script resource with the type attribute set.</returns>
        public static IScriptResource SetType(this IScriptResource input, string type)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Type = type;
            return input;
        }
    }
}
