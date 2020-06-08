// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.Assets
{
    public class ContentPage
    {
        public IFolderInfo Folder { get; set; }

        public ICollection<object> Items { get; set; }

        public int TotalCount { get; set; }
    }
}
