// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
