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
