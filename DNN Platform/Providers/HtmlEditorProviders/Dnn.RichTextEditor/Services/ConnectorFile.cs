// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System.Runtime.Serialization;

    /// <summary>A file in a folder listing.</summary>
    [DataContract]
    public class ConnectorFile
    {
        /// <summary>Gets or sets the file name.</summary>
        [DataMember(Name = "file")]
        public string File { get; set; }

        /// <summary>Gets or sets the last modification time.</summary>
        [DataMember(Name = "changed")]
        public string Changed { get; set; }

        /// <summary>Gets or sets the human readable size.</summary>
        [DataMember(Name = "size")]
        public string Size { get; set; }

        /// <summary>Gets or sets a value indicating whether the file is an image.</summary>
        [DataMember(Name = "isImage")]
        public bool IsImage { get; set; }

        /// <summary>Gets or sets the item type.</summary>
        [DataMember(Name = "type")]
        public string Type { get; set; } = "file";
    }
}
