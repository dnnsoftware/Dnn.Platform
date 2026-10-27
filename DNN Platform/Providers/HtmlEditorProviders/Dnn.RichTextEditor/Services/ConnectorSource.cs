// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>One folder's listing.</summary>
    [DataContract]
    public class ConnectorSource
    {
        /// <summary>Gets or sets the source name.</summary>
        [DataMember(Name = "name")]
        public string Name { get; set; } = "default";

        /// <summary>Gets or sets the URL prefix the browser joins with <see cref="Path"/> and a file name to build the file's URL.</summary>
        [DataMember(Name = "baseurl")]
        public string BaseUrl { get; set; }

        /// <summary>Gets or sets the folder's DNN folder path.</summary>
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// <summary>Gets or sets the subfolder names.</summary>
        [DataMember(Name = "folders")]
        public IList<string> Folders { get; set; } = new List<string>();

        /// <summary>Gets or sets the files.</summary>
        [DataMember(Name = "files")]
        public IList<ConnectorFile> Files { get; set; } = new List<ConnectorFile>();
    }
}
