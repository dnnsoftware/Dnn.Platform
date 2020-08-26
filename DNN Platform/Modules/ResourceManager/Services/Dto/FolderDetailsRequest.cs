﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Runtime.Serialization;

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class FolderDetailsRequest
    {
        [DataMember(Name = "folderId")]
        public int FolderId { get; set; }

        [DataMember(Name = "folderName")]
        public string FolderName { get; set; }

        [DataMember(Name = "permissions")]
        public FolderPermissions Permissions { get; set; }

    }
}
