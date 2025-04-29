// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Constants;

/// <summary>Link Dialog Protocol. More info https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-linkDefaultProtocol.</summary>
public enum LinkProtocol
{
    /// <summary>HTTP.</summary>
    Http = 0,

    /// <summary>Https.</summary>
    Https = 1,

    /// <summary>Ftp.</summary>
    Ftp = 2,

    /// <summary>News.</summary>
    News = 3,

    /// <summary>Other.</summary>
    Other = 9,
}
