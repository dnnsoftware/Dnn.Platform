// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using System;

using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Search;

[DnnDeprecated(7, 1, 0, "Replaced by ModuleSearchBase", RemovalVersion = 10)]
public partial interface ISearchable
{
    [Obsolete("Deprecated in DotNetNuke 7.1.0. Replaced by ModuleSearchBase.GetModifiedSearchDocuments. Scheduled for removal in v10.0.0.")]
    SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo);
}
