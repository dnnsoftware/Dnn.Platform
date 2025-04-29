// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Objects;

using System;
using System.IO;
using System.Linq;

/// <summary>FileUploadStatus Class.</summary>
public class FilesUploadStatus
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    /// <summary>The handler path.</summary>
    public const string HandlerPath = "/";

    /// <summary>Initializes a new instance of the <see cref="FilesUploadStatus"/> class.</summary>
    public FilesUploadStatus()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FilesUploadStatus"/> class.</summary>
    /// <param name="fileInfo">The file information.</param>
    public FilesUploadStatus(FileInfo fileInfo)
    {
        this.SetValues(fileInfo.Name, (int)fileInfo.Length);
    }

    /// <summary>Initializes a new instance of the <see cref="FilesUploadStatus"/> class.</summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="fileLength">Length of the file.</param>
    public FilesUploadStatus(string fileName, int fileLength)
    {
        this.SetValues(fileName, fileLength);
    }

    /// <summary>Gets or sets the group.</summary>
    /// <value>
    /// The group.
    /// </value>
    public string group { get; set; }

    /// <summary>Gets or sets the name.</summary>
    /// <value>
    /// The name.
    /// </value>
    public string name { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>
    /// The type.
    /// </value>
    public string type { get; set; }

    /// <summary>Gets or sets the size.</summary>
    /// <value>
    /// The size.
    /// </value>
    public int size { get; set; }

    /// <summary>Gets or sets the progress.</summary>
    /// <value>
    /// The progress.
    /// </value>
    public string progress { get; set; }

    /// <summary>Gets or sets the URL.</summary>
    /// <value>
    /// The URL.
    /// </value>
    public string url { get; set; }

    /// <summary>Gets or sets the thumbnail_url.</summary>
    /// <value>
    /// The thumbnail_url.
    /// </value>
    public string thumbnail_url { get; set; }

    /// <summary>Gets or sets the delete_url.</summary>
    /// <value>
    /// The delete_url.
    /// </value>
    public string delete_url { get; set; }

    /// <summary>Gets or sets the delete_type.</summary>
    /// <value>
    /// The delete_type.
    /// </value>
    public string delete_type { get; set; }

    /// <summary>Gets or sets the error.</summary>
    /// <value>
    /// The error.
    /// </value>
    public string error { get; set; }

    /// <summary>Sets the values.</summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="fileLength">Length of the file.</param>
    private void SetValues(string fileName, int fileLength)
    {
        this.name = fileName;
        this.type = "image/png";
        this.size = fileLength;
        this.progress = "1.0";
        this.url = string.Format("{0}FileTransferHandler.ashx?f={1}", HandlerPath, fileName);
    }
#pragma warning restore SA1300 // Element should begin with upper-case letter
}
