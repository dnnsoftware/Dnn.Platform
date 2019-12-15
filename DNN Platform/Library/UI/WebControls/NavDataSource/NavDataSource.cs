// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Security.Permissions;
using System.Web;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class NavDataSource : HierarchicalDataSourceControl
    {
        //Return a strongly typed view for the current data source control.
        private NavDataSourceView view;

        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            view = new NavDataSourceView(viewPath);
            return view;
        }
    }
}
