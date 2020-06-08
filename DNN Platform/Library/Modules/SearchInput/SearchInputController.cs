// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Modules.SearchInput
{
    public class SearchInputController
    {
        public ArrayList GetSearchResultModules(int PortalID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetSearchResultModules(PortalID), typeof (SearchResultsModuleInfo));
        }
    }
}
