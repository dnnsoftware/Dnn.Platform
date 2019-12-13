// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                return (PortalAliasInfo) Dictionary[key];
            }
            set
            {
                Dictionary[key] = value;
            }
        }

		/// <summary>
		/// Gets a value indicating if the collection contains keys that are not null.
		/// </summary>
        public Boolean HasKeys
        {
            get
            {
                return Dictionary.Keys.Count > 0;
            }
        }

        public ICollection Keys
        {
            get
            {
                return Dictionary.Keys;
            }
        }

        public ICollection Values
        {
            get
            {
                return Dictionary.Values;
            }
        }

        public bool Contains(String key)
        {
            return Dictionary.Contains(key);
        }

		/// <summary>
		/// Adds an entry to the collection.
		/// </summary>
        public void Add(String key, PortalAliasInfo value)
        {
            Dictionary.Add(key, value);
        }
    }
}
