// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System.Runtime.Serialization;

    /// <summary>A response in the format the editor's file browser and uploader expect.</summary>
    [DataContract]
    public class ConnectorResponse
    {
        /// <summary>Gets or sets a value indicating whether the request succeeded.</summary>
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        /// <summary>Gets or sets the server time of the response.</summary>
        [DataMember(Name = "time")]
        public string Time { get; set; }

        /// <summary>Gets or sets the response data.</summary>
        [DataMember(Name = "data")]
        public ConnectorData Data { get; set; } = new ConnectorData();
    }
}
