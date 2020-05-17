// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Services.Search;

#endregion

namespace DotNetNuke.Entities.Modules
{
     [Obsolete("Deprecated in DNN 7.1. Replaced by ModuleSearchBase. Scheduled removal in v10.0.0.")]
    public interface ISearchable
    {
        [Obsolete("Deprecated in DNN 7.1. Replaced by ModuleSearchBase.GetModifiedSearchDocuments. Scheduled removal in v10.0.0.")]
        SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo);
    }
}
