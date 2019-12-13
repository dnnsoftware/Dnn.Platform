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
