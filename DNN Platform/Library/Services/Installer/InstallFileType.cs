// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer;

using System.ComponentModel;

[TypeConverter(typeof(EnumConverter))]
public enum InstallFileType
{
    AppCode = 0,
    Ascx = 1,
    Assembly = 2,
    CleanUp = 3,
    Language = 4,
    Manifest = 5,
    Other = 6,
    Resources = 7,
    Script = 8,
}
