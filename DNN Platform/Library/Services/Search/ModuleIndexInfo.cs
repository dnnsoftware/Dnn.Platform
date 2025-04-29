// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using DotNetNuke.Entities.Modules;

internal class ModuleIndexInfo
{
    public ModuleInfo ModuleInfo { get; set; }

    public bool SupportSearch { get; set; }
}
