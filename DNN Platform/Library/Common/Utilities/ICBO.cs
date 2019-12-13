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
