// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchInput;

using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

public class SearchInputController
{
    public ArrayList GetSearchResultModules(int portalID)
    {
        return CBO.FillCollection(DataProvider.Instance().GetSearchResultModules(portalID), typeof(SearchResultsModuleInfo));
    }
}
