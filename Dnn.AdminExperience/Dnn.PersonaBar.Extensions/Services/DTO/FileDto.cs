// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class FileDto
    {
        public int fileId { get; set; }
        public string fileName { get; set; }
        public int folderId { get; set; }
        public string folderPath { get; set; }
    }
}
