// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    public class DnnFormPagesItem : DnnFormComboBoxItem
    {
        public DnnFormPagesItem()
        {
            this.ListSource = TabController.GetPortalTabs(this.PortalSettings.PortalId, Null.NullInteger, true, "<" + Localization.GetString("None_Specified") + ">", true, false, true, true, false);
            this.ListTextField = "TabName";
            this.ListValueField = "TabID";
        }
    }
}
