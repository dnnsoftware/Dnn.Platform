// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Data;

    using DotNetNuke.Data;

    /// <summary>
    /// An implementation of <see cref="IDataProvider"/>
    /// that relies on the <see cref="DataProvider"/> class.
    /// </summary>
    internal class DataProviderShim : IDataProvider
    {
        /// <inheritdoc/>
        public IDataReader ExecuteSQL(string sql)
        {
            return DataProvider.Instance().ExecuteSQL(sql);
        }
    }
}
