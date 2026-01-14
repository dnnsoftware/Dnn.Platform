// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer
{
    using System.Text;

    public enum TextEncoding
    {
        /// <summary><see cref="Encoding.UTF7"/>.</summary>
        UTF7 = 0,

        /// <summary><see cref="Encoding.UTF8"/>.</summary>
        UTF8 = 1,

        /// <summary>The big endian encoding of UTF-16.</summary>
        UTF16BigEndian = 2,

        /// <summary>The little endian encoding of UTF-16.</summary>
        UTF16LittleEndian = 3,

        /// <summary>An unknown encoding.</summary>
        Unknown = 4,
    }
}
