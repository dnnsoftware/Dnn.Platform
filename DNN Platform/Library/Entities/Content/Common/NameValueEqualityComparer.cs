// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Entities.Content.Common
{
    internal class NameValueEqualityComparer : IEqualityComparer<KeyValuePair<string, string>>
    {
        #region Implementation of IEqualityComparer<KeyValuePair<string,string>>

        public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
        {
            return x.Key == y.Key && x.Value == y.Value;
        }

        public int GetHashCode(KeyValuePair<string, string> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
        }

        #endregion
    }
}
