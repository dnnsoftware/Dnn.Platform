// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class PreviewInfoViewModel
    {
        public string Title;
        public string PreviewImageUrl;
        public int ItemId;
        public bool IsFolder;
        public List<Field> Fields;
    }
}
