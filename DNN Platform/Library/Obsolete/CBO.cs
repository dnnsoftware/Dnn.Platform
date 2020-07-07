// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
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
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr)
            where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>(), true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Obsolete in DotNetNuke 7.3.  Use FillDictionary<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary). Scheduled removal in v10.0.0.")]
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr, ref IDictionary<int, TItem> objToFill)
            where TItem : IHydratable
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
