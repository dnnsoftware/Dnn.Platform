// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using System;
    using System.Collections;

    /// <summary>
    /// The <see cref="IPortalAliasInfo"/> Collection which provides additional
    /// helper functions for managing collections of <see cref="IPortalAliasInfo"/>.
    /// </summary>
    [Serializable]
    public class PortalAliasCollection : DictionaryBase
    {
        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the collection contains keys that are not null.
        /// </summary>
        public bool HasKeys
        {
            get => this.Dictionary.Keys.Count > 0;
        }

        /// <summary>
        /// Gets all the keys in the collection.
        /// </summary>
        public ICollection Keys
        {
            get => this.Dictionary.Keys;
        }

        /// <summary>
        /// Gets all the values in the collection.
        /// </summary>
        public ICollection Values
        {
            get => this.Dictionary.Values;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key value to retrieve.</param>
        /// <returns>The Portal Alias Info.</returns>
        public IPortalAliasInfo this[string key]
        {
            get => (IPortalAliasInfo)this.Dictionary[key];
            set => this.Dictionary[key] = value;
        }

        /// <summary>
        /// Determines if the key exists in the collection.
        /// </summary>
        /// <param name="key">The input key name.</param>
        /// <returns>True if the key exists in the collection.</returns>
        public bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Adds an entry to the collection.
        /// </summary>
        /// <param name="key">The key to associate with the new entry.</param>
        /// <param name="value">The portal alias.</param>
        public void Add(string key, IPortalAliasInfo value)
        {
            this.Dictionary.Add(key, value);
        }
    }
}
