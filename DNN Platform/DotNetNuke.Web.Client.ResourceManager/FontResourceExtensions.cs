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
        /// <param name="sourcePath">The source URL to set.</param>
        /// <returns>The resource with the source URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IFontResource"/>.</typeparam>
        public static T FromSrc<T>(this T input, string sourcePath)
            where T : IFontResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FilePath = sourcePath;
            switch (input.FilePath?.Substring(input.FilePath.LastIndexOf('.')).ToLowerInvariant())
            {
                case ".eot":
                    input.Type = "application/vnd.ms-fontobject";
                    break;
                case ".woff":
                    input.Type = "font/woff";
                    break;
                case ".woff2":
                    input.Type = "font/woff2";
                    break;
                case ".ttf":
                    input.Type = "font/ttf";
                    break;
                case ".svg":
                    input.Type = "image/svg+xml";
                    break;
                case ".otf":
                    input.Type = "font/otf";
                    break;
                default:
                    input.Type = "application/octet-stream";
                    break;
            }

            return input;
        }

        /// <summary>
        /// Sets the source URL of the resource.
        /// </summary>
        /// <param name="input">The resource to set the source URL for.</param>
        /// <param name="sourcePath">The source URL to set.</param>
        /// <param name="pathNameAlias">The path alias to set.</param>
        /// <param name="mimeType">The MIME type of the resource.</param>
        /// <returns>The resource with the source URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IFontResource"/>.</typeparam>
        public static T FromSrc<T>(this T input, string sourcePath, string pathNameAlias, string mimeType)
            where T : IFontResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FilePath = sourcePath;
            input.PathNameAlias = pathNameAlias;
            input.Type = mimeType;
            return input;
        }
    }
}
