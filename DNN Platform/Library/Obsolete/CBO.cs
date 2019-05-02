#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    /// The CBO class generates objects.
    /// </summary>
    public partial class CBO
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Use CreateObject<T>(bool). Scheduled removal in v10.0.0.")]
        public static TObject CreateObject<TObject>()
        {
            return (TObject)CreateObjectInternal(typeof(TObject), false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Use CreateObject<T>(bool). Scheduled removal in v10.0.0.")]
        public static object CreateObject(Type objType, bool initialise)
        {
            return CreateObjectInternal(objType, initialise);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Use FillDictionary<TKey, TValue>(string keyField, IDataReader dr). Scheduled removal in v10.0.0.")]
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>(), true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Use FillDictionary<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary). Scheduled removal in v10.0.0.")]
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr, ref IDictionary<int, TItem> objToFill) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, objToFill, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Replaced by FillObject<T> . Scheduled removal in v10.0.0.")]
        public static object FillObject(IDataReader dr, Type objType)
        {
            return CreateObjectFromReader(objType, dr, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Replaced by FillObject<T> . Scheduled removal in v10.0.0.")]
        public static object FillObject(IDataReader dr, Type objType, bool closeReader)
        {
            return CreateObjectFromReader(objType, dr, closeReader);
        }
    }
}
