#region Usings

using System;
using System.Collections.Generic;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNPageEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNPageEditControl control provides a standard UI component for selecting
    /// a DNN Page
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNPageEditControl runat=server></{0}:DNNPageEditControl>")]
    public class DNNPageEditControl : IntegerEditControl
    {
	#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            PortalSettings _portalSettings = Globals.GetPortalSettings();

            //Get the Pages
            List<TabInfo> listTabs = TabController.GetPortalTabs(_portalSettings.PortalId, Null.NullInteger, true, "<" + Localization.GetString("None_Specified") + ">", true, false, true, true, false);

            //Render the Select Tag
            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int tabIndex = 0; tabIndex <= listTabs.Count - 1; tabIndex++)
            {
                TabInfo tab = listTabs[tabIndex];

                //Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, tab.TabID.ToString());

                if (tab.TabID == IntegerValue)
                {
					//Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
				
                //Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(tab.IndentedTabName);
                writer.RenderEndTag();
            }
			
            //Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            TabInfo linkedTabInfo = TabController.Instance.GetTab(IntegerValue, Globals.GetPortalSettings().PortalId, false);

            //don't render anything if we didn't find the tab
            if (linkedTabInfo != null)
            {
                //Not really sure how to get a good TabID and ModuleID but it's only for tracking so not to concerned
                int tabID = 0;
                int moduleID = 0;
                Int32.TryParse(Page.Request.QueryString["tabid"], out tabID);
                Int32.TryParse(Page.Request.QueryString["mid"], out moduleID);

                string url = Globals.LinkClick(StringValue, tabID, moduleID, true);
                ControlStyle.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "Normal");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(linkedTabInfo.LocalizedTabName);
                writer.RenderEndTag();
            }
        }
		
		#endregion
    }
}
