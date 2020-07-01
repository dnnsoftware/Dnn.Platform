// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileChangedEventArgs : System.EventArgs
    {
        public IFileInfo FileInfo { get; set; }

        public int UserId { get; set; }
    }
}
