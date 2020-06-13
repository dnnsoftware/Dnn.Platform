// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;

    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class NavDataSource : HierarchicalDataSourceControl
    {
        // Return a strongly typed view for the current data source control.
        private NavDataSourceView view;

        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            this.view = new NavDataSourceView(viewPath);
            return this.view;
        }
    }
}
