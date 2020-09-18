// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System.ComponentModel;

    [TypeConverter(typeof(EnumConverter))]
    public enum InstallMode
    {
        Install,
        ManifestOnly,
        UnInstall,
    }
}
