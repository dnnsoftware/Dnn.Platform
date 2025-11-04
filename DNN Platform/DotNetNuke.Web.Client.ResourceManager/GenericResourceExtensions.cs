// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IResource"/> to set various resource properties in a fluent manner.
    /// </summary>
    public static class GenericResourceExtensions
    {
        /// <summary>
        /// Sets the priority of the resource.
        /// </summary>
        /// <param name="input">The resource to set the priority for.</param>
        /// <param name="priority">The priority value to set.</param>
        /// <returns>The resource with the priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPriority<T>(this T input, int priority)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Priority = priority;
            return input;
        }

        /// <summary>
        /// Sets the name, version, and force version flag of the resource.
        /// </summary>
        /// <param name="input">The resource to set the name and version for.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="version">The version to set.</param>
        /// <param name="forceVersion">Whether to force the version.</param>
        /// <returns>The resource with the name and version set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetNameAndVersion<T>(this T input, string name, string version, bool forceVersion)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Name = name;
            input.Version = version;
            input.ForceVersion = forceVersion;
            return input;
        }

        /// <summary>
        /// Sets the CDN URL of the resource.
        /// </summary>
        /// <param name="input">The resource to set the CDN URL for.</param>
        /// <param name="cdnUrl">The CDN URL to set.</param>
        /// <returns>The resource with the CDN URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetCdnUrl<T>(this T input, string cdnUrl)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.CdnUrl = cdnUrl;
            return input;
        }

        /// <summary>
        /// Sets the provider of the resource.
        /// </summary>
        /// <param name="input">The resource to set the provider for.</param>
        /// <param name="provider">The provider to set.</param>
        /// <returns>The resource with the provider set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetProvider<T>(this T input, string provider)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Provider = provider;
            return input;
        }

        /// <summary>
        /// Marks the resource as blocking.
        /// </summary>
        /// <param name="input">The resource to mark as blocking.</param>
        /// <returns>The resource marked as blocking.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetBlocking<T>(this T input)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Blocking = true;
            return input;
        }

        /// <summary>
        /// Sets the integrity hash of the resource.
        /// </summary>
        /// <param name="input">The resource to set the integrity hash for.</param>
        /// <param name="hash">The integrity hash to set.</param>
        /// <returns>The resource with the integrity hash set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetIntegrity<T>(this T input, string hash)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.Integrity = hash;
            return input;
        }

        /// <summary>
        /// Sets the Cross-Origin attribute of the resource.
        /// </summary>
        /// <param name="input">The resource to set the Cross-Origin attribute for.</param>
        /// <param name="crossOrigin">The Cross-Origin value to set.</param>
        /// <returns>The resource with the Cross-Origin attribute set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetCrossOrigin<T>(this T input, CrossOrigin crossOrigin)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.CrossOrigin = crossOrigin;
            return input;
        }

        /// <summary>
        /// Sets the fetch priority of the resource.
        /// </summary>
        /// <param name="input">The resource to set the fetch priority for.</param>
        /// <param name="fetchPriority">The fetch priority to set.</param>
        /// <returns>The resource with the fetch priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetFetchPriority<T>(this T input, FetchPriority fetchPriority)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.FetchPriority = fetchPriority;
            return input;
        }

        /// <summary>
        /// Sets the referrer policy of the resource.
        /// </summary>
        /// <param name="input">The resource to set the referrer policy for.</param>
        /// <param name="referrerPolicy">The referrer policy to set.</param>
        /// <returns>The resource with the referrer policy set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetReferrerPolicy<T>(this T input, ReferrerPolicy referrerPolicy)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input.ReferrerPolicy = referrerPolicy;
            return input;
        }

        /// <summary>
        /// Adds or updates an attribute of the resource.
        /// </summary>
        /// <param name="input">The resource to add or update the attribute for.</param>
        /// <param name="attributeName">The name of the attribute to add or update.</param>
        /// <param name="attributeValue">The value of the attribute to add or update.</param>
        /// <returns>The resource with the attribute added or updated.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T AddAttribute<T>(this T input, string attributeName, string attributeValue)
            where T : IResource
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!string.IsNullOrEmpty(attributeName))
            {
                if (input.Attributes.ContainsKey(attributeName))
                {
                    input.Attributes[attributeName] = attributeValue;
                }
                else
                {
                    input.Attributes.Add(attributeName, attributeValue);
                }
            }

            return input;
        }
    }
}
