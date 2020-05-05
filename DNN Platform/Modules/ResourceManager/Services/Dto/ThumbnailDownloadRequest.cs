// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class ThumbnailDownloadRequest
    {
        public int FileId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int? Version { get; set; }
    }
}