// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System.ComponentModel;

    [TypeConverter(typeof(EnumConverter))]
    public enum InstallFileType
    {
        /// <summary>An <c>App_Code</c> file.</summary>
        AppCode = 0,

        /// <summary>An ascx file.</summary>
        Ascx = 1,

        /// <summary>A DLL file.</summary>
        Assembly = 2,

        /// <summary>A clean-up file.</summary>
        CleanUp = 3,

        /// <summary>A resx file.</summary>
        Language = 4,

        /// <summary>A manifest file.</summary>
        Manifest = 5,

        /// <summary>Another type of file.</summary>
        Other = 6,

        /// <summary>A resources file.</summary>
        Resources = 7,

        /// <summary>A SQL script file.</summary>
        Script = 8,
    }
}
