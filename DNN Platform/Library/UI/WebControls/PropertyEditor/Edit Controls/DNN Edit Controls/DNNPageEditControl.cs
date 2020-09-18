// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNPageEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNPageEditControl control provides a standard UI component for selecting
    /// a DNN Page.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNPageEditControl runat=server></{0}:DNNPageEditControl>")]
    public class DNNPageEditControl : IntegerEditControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            PortalSettings _portalSettings = Globals.GetPortalSettings();

            // Get the Pages
            List<TabInfo> listTabs = TabController.GetPortalTabs(_portalSettings.PortalId, Null.NullInteger, true, "<" + Localization.GetString("None_Specified") + ">", true, false, true, true, false);

            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int tabIndex = 0; tabIndex <= listTabs.Count - 1; tabIndex++)
            {
                TabInfo tab = listTabs[tabIndex];

                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, tab.TabID.ToString());

                if (tab.TabID == this.IntegerValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(tab.IndentedTabName);
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            TabInfo linkedTabInfo = TabController.Instance.GetTab(this.IntegerValue, Globals.GetPortalSettings().PortalId, false);

            // don't render anything if we didn't find the tab
            if (linkedTabInfo != null)
            {
                // Not really sure how to get a good TabID and ModuleID but it's only for tracking so not to concerned
                int tabID = 0;
                int moduleID = 0;
                int.TryParse(this.Page.Request.QueryString["tabid"], out tabID);
                int.TryParse(this.Page.Request.QueryString["mid"], out moduleID);

                string url = Globals.LinkClick(this.StringValue, tabID, moduleID, true);
                this.ControlStyle.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "Normal");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(linkedTabInfo.LocalizedTabName);
                writer.RenderEndTag();
            }
        }
    }
}
