// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>The CBO class generates objects.</summary>
public partial class CBO
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use CreateObject<T>(bool)", RemovalVersion = 10)]
    public static partial TObject CreateObject<TObject>()
    {
        return (TObject)CreateObjectInternal(typeof(TObject), false);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use CreateObject<T>(bool)", RemovalVersion = 10)]
    public static partial object CreateObject(Type objType, bool initialise)
    {
        return CreateObjectInternal(objType, initialise);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use FillDictionary<TKey, TValue>(string keyField, IDataReader dr)", RemovalVersion = 10)]
    public static partial IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr)
        where TItem : IHydratable
    {
        return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>(), true);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use FillDictionary<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary)", RemovalVersion = 10)]
    public static partial IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr, ref IDictionary<int, TItem> objToFill)
        where TItem : IHydratable
    {
        return FillDictionaryFromReader("KeyID", dr, objToFill, true);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by FillObject<T> ", RemovalVersion = 10)]
    public static partial object FillObject(IDataReader dr, Type objType)
    {
        return CreateObjectFromReader(objType, dr, true);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by FillObject<T> ", RemovalVersion = 10)]
    public static partial object FillObject(IDataReader dr, Type objType, bool closeReader)
    {
        return CreateObjectFromReader(objType, dr, closeReader);
    }
}
