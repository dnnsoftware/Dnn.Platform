// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Constants;

/// <summary>The Language Direction.</summary>
public enum LanguageDirection
{
    /// <summary>Indicate content direction will be the same with either the editor UI direction or page element direction depending on the creators.</summary>
    Inherit = 0,

    /// <summary>Language Direction Left to Right.</summary>
    LeftToRight = 1,

    /// <summary>Language Direction Right to Left.</summary>
    RightToLeft = 2,
}
