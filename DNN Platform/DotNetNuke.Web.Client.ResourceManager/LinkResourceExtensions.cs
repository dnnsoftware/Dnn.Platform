// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="ILinkResource"/> to set various resource properties in a fluent manner.
    /// </summary>
    public static class LinkResourceExtensions
    {
        /// <summary>
        /// Marks the resource for preload.
        /// </summary>
        /// <param name="input">The resource to mark for preload.</param>
        /// <returns>The resource marked for preload.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPreload<T>(this T input)
            where T : ILinkResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Preload = true;
            return input;
        }
    }
}
