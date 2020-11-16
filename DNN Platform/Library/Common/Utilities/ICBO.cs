// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Collections.Generic;
    using System.Data;

    public interface ICBO
    {
        List<TObject> FillCollection<TObject>(IDataReader dr)
            where TObject : new();

        TObject FillObject<TObject>(IDataReader dr)
            where TObject : new();

        // SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr);
        TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary);
    }
}
