// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>The data of a <see cref="ConnectorResponse"/>.</summary>
    [DataContract]
    public class ConnectorData
    {
        /// <summary>Gets or sets the status code (220 for success, as the browser expects).</summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        /// <summary>Gets the messages to show the user.</summary>
        [DataMember(Name = "messages")]
        public IList<string> Messages { get; } = new List<string>();

        /// <summary>Gets or sets the folder listings (folders and files actions).</summary>
        [DataMember(Name = "sources", EmitDefaultValue = false)]
        public IList<ConnectorSource> Sources { get; set; }

        /// <summary>Gets or sets the user's permissions in the current folder (permissions action).</summary>
        [DataMember(Name = "permissions", EmitDefaultValue = false)]
        public IDictionary<string, bool> Permissions { get; set; }

        /// <summary>Gets or sets the URLs of the uploaded files (upload).</summary>
        [DataMember(Name = "files", EmitDefaultValue = false)]
        public IList<string> Files { get; set; }

        /// <summary>Gets or sets which uploaded files are images (upload).</summary>
        [DataMember(Name = "isImages", EmitDefaultValue = false)]
        public IList<bool> IsImages { get; set; }

        /// <summary>Gets or sets the prefix for <see cref="Files"/>; empty because the URLs are complete (upload).</summary>
        [DataMember(Name = "baseurl", EmitDefaultValue = false)]
        public string BaseUrl { get; set; }
    }
}
