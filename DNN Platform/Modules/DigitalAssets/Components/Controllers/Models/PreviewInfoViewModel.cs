// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    using System;
    using System.Collections.Generic;

    public class PreviewInfoViewModel
    {
        public string Title;
        public string PreviewImageUrl;
        public int ItemId;
        public bool IsFolder;
        public List<Field> Fields;
    }
}
