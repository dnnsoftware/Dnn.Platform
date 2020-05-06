// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Net.Http;

namespace Dnn.Modules.ResourceManager.Components.Models
{
    public class ThumbnailContent
    {
        public ByteArrayContent Content { get; set; }
        public string ContentType { get; set; }
        public int Height { get; set; }
        public string ThumbnailName { get; set; }
        public int Width { get; set; }
    }
}