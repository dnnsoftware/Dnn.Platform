// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Dto;

using Newtonsoft.Json;

/// <summary>Specifies page to be exported.</summary>
[JsonObject]
public class PageToExport
{
    /// <summary>Gets or sets the tab ID.</summary>
    public int TabId { get; set; }

    /// <summary>Gets or sets the parent tab ID.</summary>
    public int ParentTabId { get; set; }

    /// <summary>Gets or sets the checked state.</summary>
    public TriCheckedState CheckedState { get; set; }
}
