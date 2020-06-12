
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;

using DotNetNuke.Services.Search;

namespace DotNetNuke.Entities.Modules
{
     [Obsolete("Deprecated in DNN 7.1. Replaced by ModuleSearchBase. Scheduled removal in v10.0.0.")]
    public interface ISearchable
    {
        [Obsolete("Deprecated in DNN 7.1. Replaced by ModuleSearchBase.GetModifiedSearchDocuments. Scheduled removal in v10.0.0.")]
        SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo);
    }
}
