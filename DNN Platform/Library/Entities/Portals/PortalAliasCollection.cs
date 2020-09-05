// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;

    [Serializable]
    [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
    public class PortalAliasCollection : DictionaryBase
    {
        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the collection contains keys that are not null.
        /// </summary>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public bool HasKeys
        {
            get
            {
                return this.Dictionary.Keys.Count > 0;
            }
        }

        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public ICollection Values
        {
            get
            {
                return this.Dictionary.Values;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public PortalAliasInfo this[string key]
        {
            get
            {
                return (PortalAliasInfo)this.Dictionary[key];
            }

            set
            {
                this.Dictionary[key] = value;
            }
        }

        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Adds an entry to the collection.
        /// </summary>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use IDictionary<string, IPortalAliasInfo> instead.")]
        public void Add(string key, PortalAliasInfo value)
        {
            this.Dictionary.Add(key, value);
        }
    }
}
