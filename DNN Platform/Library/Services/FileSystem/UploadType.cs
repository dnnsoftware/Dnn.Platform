// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    public enum UploadType
    {
        /// <summary>Upload a file.</summary>
        File = 0,

        /// <summary>Upload a container file.</summary>
        Container = 1,

        /// <summary>Upload a skin file.</summary>
        Skin = 2,

        /// <summary>Upload a module file.</summary>
        Module = 3,

        /// <summary>Upload a language page file.</summary>
        LanguagePack = 4,
    }
}
