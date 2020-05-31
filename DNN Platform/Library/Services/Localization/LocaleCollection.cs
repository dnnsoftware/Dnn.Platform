// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.Collections.Specialized;

#endregion

namespace DotNetNuke.Services.Localization
{
    /// <summary>
    /// <para>The LocaleCollection class is a collection of Locale objects.  It stores the supported locales.</para>
    /// </summary>
    public class LocaleCollection : NameObjectCollectionBase
    {
        private DictionaryEntry _de;

        public DictionaryEntry this[int index]
        {
            get
            {
                _de.Key = BaseGetKey(index);
                _de.Value = BaseGet(index);
                return _de;
            }
        }

        //Gets or sets the value associated with the specified key.
        public Locale this[string key]
        {
            get
            {
                return (Locale) BaseGet(key);
            }
            set
            {
                BaseSet(key, value);
            }
        }

        //Gets a String array that contains all the keys in the collection.
        public string[] AllKeys
        {
            get
            {
                return BaseGetAllKeys();
            }
        }

        //Gets an Object array that contains all the values in the collection.
        public Array AllValues
        {
            get
            {
                return BaseGetAllValues();
            }
        }

        //Gets a value indicating if the collection contains keys that are not null.
        public Boolean HasKeys
        {
            get
            {
                return BaseHasKeys();
            }
        }

        //Adds an entry to the collection.
        public void Add(String key, Object value)
        {
            BaseAdd(key, value);
        }

        //Removes an entry with the specified key from the collection.
        public void Remove(String key)
        {
            BaseRemove(key);
        }
    }
}
