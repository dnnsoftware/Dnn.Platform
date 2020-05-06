// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Runtime.Serialization;

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class FileDetailsRequest
    {
        [DataMember(Name = "fileId")]
        public int FileId { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}