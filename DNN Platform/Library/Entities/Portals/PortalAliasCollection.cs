// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Entities.Portals
{
    [Serializable]
    public class PortalAliasCollection : DictionaryBase
    {
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating if the collection contains keys that are not null.
        /// </summary>
        public Boolean HasKeys
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

        public bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Adds an entry to the collection.
        /// </summary>
        public void Add(String key, PortalAliasInfo value)
        {
            this.Dictionary.Add(key, value);
        }
    }
}
