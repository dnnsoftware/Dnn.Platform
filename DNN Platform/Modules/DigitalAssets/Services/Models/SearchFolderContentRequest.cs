// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class SearchFolderContentRequest
    {
        public int FolderId { get; set; }

        public string Pattern { get; set; }

        public int StartIndex { get; set; }

        public int NumItems { get; set; }

        public string SortExpression { get; set; }
    }
}
