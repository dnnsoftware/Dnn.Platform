// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IStylesheetResource"/> to set various stylesheet resource properties in a fluent manner.
    /// </summary>
    public static class StylesheetResourceExtensions
    {
        /// <summary>
        /// Sets the source URL of the resource.
        /// </summary>
        /// <param name="input">The resource to set the source URL for.</param>
        /// <param name="sourcePath">The source URL to set.</param>
        /// <returns>The resource with the source URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IStylesheetResource"/>.</typeparam>
        public static T FromSrc<T>(this T input, string sourcePath)
            where T : IStylesheetResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FilePath = sourcePath;
            return input;
        }

        /// <summary>
        /// Sets the source URL and path alias of the resource.
        /// </summary>
        /// <param name="input">The resource to set the source URL and path alias for.</param>
        /// <param name="sourcePath">The source URL to set.</param>
        /// <param name="pathNameAlias">The path alias to set.</param>
        /// <returns>The resource with the source URL and path alias set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IStylesheetResource"/>.</typeparam>
        public static T FromSrc<T>(this T input, string sourcePath, string pathNameAlias)
            where T : IStylesheetResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FilePath = sourcePath;
            input.PathNameAlias = pathNameAlias;
            return input;
        }

        /// <summary>
        /// Sets the disabled attribute of the stylesheet resource.
        /// </summary>
        /// <param name="input">The stylesheet resource to set the disabled attribute for.</param>
        /// <returns>The stylesheet resource with the disabled attribute set.</returns>
        public static IStylesheetResource SetDisabled(this IStylesheetResource input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Disabled = true;
            return input;
        }
    }
}
