using System;
using System.Collections.Generic;

using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetOpenAuth.Messaging.Bindings;
using OAuth.AuthorizationServer.Core.Data.Extensions;
using OAuth.AuthorizationServer.Core.Data.Model;

namespace OAuth.AuthorizationServer.Core.Data.Repositories
{
    // Used by DotNetOpenAuth to track these keys
    class SymmetricCryptoKeyRepository :  ICryptoKeyStore
    {
        //public SymmetricCryptoKeyRepository()
        //    : base(new OAuthDataContext())
        //{
        //}

        //public SymmetricCryptoKeyRepository(IObjectContextAdapter context)
        //    : base(context)
        //{
        //}

        //public CryptoKey GetKey(string bucket, string handle)
        //{
        //    List<SymmetricCryptoKey> keys = Context.SymmetricCryptoKeys.Where(k => k.Bucket == bucket && k.Handle == handle).ToList();
        //    // Perform a case senstive match
        //    IEnumerable<CryptoKey> matches = from key in keys
        //                                     where string.Equals(key.Bucket, bucket, StringComparison.Ordinal)
        //                                     && string.Equals(key.Handle, handle, StringComparison.Ordinal)
        //                                     select new CryptoKey(key.Secret, key.ExpiresUtc.AsUtc());
        //    return matches.FirstOrDefault();
        //}

        //public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
        //{
        //    var keys = Context.SymmetricCryptoKeys.Where(key => key.Bucket == bucket).OrderByDescending(x => x.ExpiresUtc).ToList()
        //        .Select(key => new KeyValuePair<string, CryptoKey>(key.Handle, new CryptoKey(key.Secret, key.ExpiresUtc.AsUtc())));
        //    return keys.ToList();
        //}

        //public void StoreKey(string bucket, string handle, CryptoKey key)
        //{
        //    SymmetricCryptoKey keyRow = new SymmetricCryptoKey
        //    {
        //        Bucket = bucket,
        //        Handle = handle,
        //        Secret = key.Key,
        //        ExpiresUtc = key.ExpiresUtc
        //    };

        //    Context.SymmetricCryptoKeys.Add(keyRow);
        //    Context.SaveChanges();
        //}

        //public void RemoveKey(string bucket, string handle)
        //{
        //    SymmetricCryptoKey match = Context.SymmetricCryptoKeys.FirstOrDefault(k => k.Bucket == bucket && k.Handle == handle);
        //    if (match != null)
        //    {
        //        Context.SymmetricCryptoKeys.Remove(match);
        //        Context.SaveChanges();
        //    }
        //}
        public CryptoKey GetKey(string bucket, string handle)
        {
            return CBO.FillObject<CryptoKey>(DataProvider.Instance().ExecuteReader("SCKRGetKey", bucket, handle));
        }

        public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
        {

            return CBO.FillCollection<KeyValuePair<string, CryptoKey>>(DataProvider.Instance().ExecuteReader("SCKRGetKeys", bucket));
        }

        public void StoreKey(string bucket, string handle, CryptoKey key)
        {
            DataProvider.Instance().ExecuteNonQuery("StoreKey", bucket, handle, key.ExpiresUtc, key.Key.ToString());
        }

        public void RemoveKey(string bucket, string handle)
        {
            DataProvider.Instance().ExecuteNonQuery("SCKRRemoveKey", bucket, handle);
        }
    }
}
