#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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