// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Constants;

/// <summary>The Default Link Type.</summary>
public enum LinkType
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    /// <summary>Link Type URL.</summary>
    url = 0,

    /// <summary>Link Type local Page.</summary>
    localPage = 1,

    /// <summary>Link Type anchor.</summary>
    anchor = 2,

    /// <summary>Link Type email.</summary>
    email = 3,
#pragma warning restore SA1300 // Element should begin with upper-case letter
}
