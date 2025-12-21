// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI.WebControls;

    public interface IGridColumnExtensionPoint : IExtensionPoint
    {
        int ColumnAt { get; }

        string UniqueName { get; }

        string DataField { get; }

        string HeaderText { get; }

        Unit HeaderStyleWidth { get; }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        bool ReadOnly { get; }

        bool Reorderable { get; }

        string SortExpression { get; }
    }
}
