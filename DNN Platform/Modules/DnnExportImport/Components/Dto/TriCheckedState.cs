// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Dto;

/// <summary>Three check states.</summary>
public enum TriCheckedState
{
#if false
        // for schema 1.0.0
        Checked = 0,
        UnChecked = 1,
        Partial = 2,
#else
    /// <summary>Unchecked.</summary>
    UnChecked = 0,

    /// <summary>Checked.</summary>
    Checked = 1,

    /// <summary>Checked with all children.</summary>
    CheckedWithAllChildren = 2,
#endif

}
