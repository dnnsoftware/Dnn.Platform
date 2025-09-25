// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A collection of <see cref="PortalAliasInfo"/> instances, indexed by <see cref="PortalAliasInfo.HttpAlias"/>.</summary>
    [Serializable]
    [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
    public partial class PortalAliasCollection : DictionaryBase
    {
        /// <summary>Gets a value indicating whether the collection contains keys that are not null.</summary>
        public bool HasKeys
        {
            get
            {
                return this.Dictionary.Keys.Count > 0;
            }
        }

        public ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        public ICollection Values
        {
            get
            {
                return this.Dictionary.Values;
            }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The HTTP alias.</param>
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

        /// <inheritdoc cref="IDictionary.Contains"/>
        [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
        public partial bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>Adds an entry to the collection.</summary>
        /// <param name="key">The <see cref="string"/> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="PortalAliasInfo"/> to use as the value of the element to add.</param>
        [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
        public partial void Add(string key, PortalAliasInfo value)
        {
            this.Dictionary.Add(key, value);
        }
    }
}
