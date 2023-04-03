// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Data;

    using DotNetNuke.Data;

    /// <summary>An abstraction of the <see cref="DataProvider"/> class to enable DI and unit testing.</summary>
    internal interface IDataProvider
    {
        /// <summary>Executes a SQL command.</summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>A <see cref="IDataReader"/> with the results of the command execution.</returns>
        IDataReader ExecuteSQL(string sql);
    }
}
