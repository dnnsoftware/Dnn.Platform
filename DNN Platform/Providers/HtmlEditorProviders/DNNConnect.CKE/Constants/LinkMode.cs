// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Constants;

/// <summary>File Browser Link Modes.</summary>
public enum LinkMode
{
    /// <summary>Relative URL.</summary>
    RelativeURL = 0,

    /// <summary>Absolute URL.</summary>
    AbsoluteURL = 1,

    /// <summary>Relative Secured URL.</summary>
    RelativeSecuredURL = 2,

    /// <summary>Absolute Secured URL.</summary>
    AbsoluteSecuredURL = 3,
}
