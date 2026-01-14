// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchInput
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    public class SearchInputController
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ArrayList GetSearchResultModules(int portalID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetSearchResultModules(portalID), typeof(SearchResultsModuleInfo));
        }
    }
}
