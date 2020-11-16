// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    /// <summary>
    /// <para>The LocaleCollection class is a collection of Locale objects.  It stores the supported locales.</para>
    /// </summary>
    public class LocaleCollection : NameObjectCollectionBase
    {
        private DictionaryEntry _de;

        // Gets a String array that contains all the keys in the collection.
        public string[] AllKeys
        {
            get
            {
                return this.BaseGetAllKeys();
            }
        }

        // Gets an Object array that contains all the values in the collection.
        public Array AllValues
        {
            get
            {
                return this.BaseGetAllValues();
            }
        }

        // Gets a value indicating if the collection contains keys that are not null.
        public bool HasKeys
        {
            get
            {
                return this.BaseHasKeys();
            }
        }

        public DictionaryEntry this[int index]
        {
            get
            {
                this._de.Key = this.BaseGetKey(index);
                this._de.Value = this.BaseGet(index);
                return this._de;
            }
        }

        // Gets or sets the value associated with the specified key.
        public Locale this[string key]
        {
            get
            {
                return (Locale)this.BaseGet(key);
            }

            set
            {
                this.BaseSet(key, value);
            }
        }

        // Adds an entry to the collection.
        public void Add(string key, object value)
        {
            this.BaseAdd(key, value);
        }

        // Removes an entry with the specified key from the collection.
        public void Remove(string key)
        {
            this.BaseRemove(key);
        }
    }
}
