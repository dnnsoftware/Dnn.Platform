// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Data;


namespace DotNetNuke.Common.Utilities
{
    public interface ICBO
    {
        List<TObject> FillCollection<TObject>(IDataReader dr) where TObject : new();

        TObject FillObject<TObject>(IDataReader dr) where TObject : new();

        //SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr);

        TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary);
    }
}
