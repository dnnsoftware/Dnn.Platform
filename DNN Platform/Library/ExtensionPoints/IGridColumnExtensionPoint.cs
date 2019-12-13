// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.UI.WebControls;

namespace DotNetNuke.ExtensionPoints
{
    public interface IGridColumnExtensionPoint : IExtensionPoint
    {
        int ColumnAt { get; }
        string UniqueName { get; }
        string DataField { get; }
        string HeaderText { get; }
        Unit HeaderStyleWidth { get; }
        bool ReadOnly { get; }
        bool Reorderable { get; }
        string SortExpression { get; }
    }
}
