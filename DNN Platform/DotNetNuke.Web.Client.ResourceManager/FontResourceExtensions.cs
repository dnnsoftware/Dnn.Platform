// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IFontResource"/> to set various resource properties in a fluent manner.
    /// </summary>
    public static class FontResourceExtensions
    {
        /// <summary>
        /// Sets the source URL of the resource.
        /// </summary>
        /// <param name="input">The resource to set the source URL for.</param>
        /// <param name="scriptSrc">The source URL to set.</param>
        /// <param name="mimeType">The MIME type of the resource.</param>
        /// <returns>The resource with the source URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T FromSrc<T>(this T input, string scriptSrc, string mimeType)
            where T : IFontResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FilePath = scriptSrc;
            input.Type = mimeType;
            return input;
        }
    }
}
