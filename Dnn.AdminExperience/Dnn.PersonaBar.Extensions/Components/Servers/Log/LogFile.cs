// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.Log
{
    using System;

    using DotNetNuke.Services.FileSystem;

    /// <summary>Represents a log file returned by the APIs of <see cref="LogController"/>.</summary>
    public class LogFile
    {
        /// <summary>Gets or sets the file name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the last write time in UTC for the file.</summary>
        public DateTime LastWriteTimeUtc { get; set; }

        /// <summary>Gets or sets the size of the file (e.g. "2.0 B" or "12.1 KB" as formatted by <see cref="FileSizeFormatProvider"/>).</summary>
        public string Size { get; set; }
    }
}
