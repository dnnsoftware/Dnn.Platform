// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System.Data;

    /// <summary>Enables arbitrary SQL execution.</summary>
    internal interface IExecuteSqlStep : IStep
    {
        /// <summary>Gets or sets the SQL command to execute.</summary>
        string CommandText { get; set; }

        /// <summary>Gets an instance of <see cref="IDataReader"/> with the results of the command execution.</summary>
        IDataReader Result { get; }
    }
}
