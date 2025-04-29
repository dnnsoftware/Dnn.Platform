// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services;

/// <summary>Represents the status of a file.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1300:Element should begin with upper-case letter",
    Justification = "Acceptable in the context of a DTO to not cause an unneccessary breaking change.")]
public class FilesStatus
{
    /// <summary>Initializes a new instance of the <see cref="FilesStatus"/> class.</summary>
    public FilesStatus()
    {
    }

    /// <summary>Gets or sets a value indicating whether the upload succeeded.</summary>
    public bool success { get; set; }

    /// <summary>Gets or sets the file name.</summary>
    public string name { get; set; }

    /// <summary>Gets or sets the file extension.</summary>
    public string extension { get; set; }

    /// <summary>Gets or sets the file type.</summary>
    public string type { get; set; }

    /// <summary>Gets or sets the file size.</summary>
    public int size { get; set; }

    /// <summary>Gets or sets the upload progress.</summary>
    public string progress { get; set; }

    /// <summary>Gets or sets the file url.</summary>
    public string url { get; set; }

    /// <summary>Gets or sets the thumbnail url.</summary>
    public string thumbnail_url { get; set; }

    /// <summary>Gets or sets the message.</summary>
    public string message { get; set; }

    /// <summary>Gets or sets the unique id.</summary>
    public int id { get; set; }
}
