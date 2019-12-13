#region Usings

using System;
using DotNetNuke.Framework;

#endregion

#region Usings

using DotNetNuke.Framework.JavaScriptLibraries;
using Telerik.Web.UI;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGrid : RadGrid
    {

        #region public properties

        public int ScreenRowNumber { get; set; }

        public int RowHeight { get; set; }

        #endregion
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.EnableEmbeddedBaseStylesheet = false;
            Utilities.ApplySkin(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            if (string.IsNullOrEmpty(ClientSettings.ClientEvents.OnGridCreated))
            {
                ClientSettings.ClientEvents.OnGridCreated = "$.dnnGridCreated";
            }

            this.PreRender += new EventHandler(DnnGrid_PreRender);

            this.MasterTableView.NoMasterRecordsText = Localization.GetString("NoRecords", Localization.SharedResourceFile);
        }

        void DnnGrid_PreRender(object sender, EventArgs e)
        {
            var items = this.MasterTableView.Items;
            if (ScreenRowNumber == 0)
                ScreenRowNumber = 15;

            if (items.Count > ScreenRowNumber)
            {
                // need scroll
                this.ClientSettings.Scrolling.AllowScroll = true;
                this.ClientSettings.Scrolling.UseStaticHeaders = true;

                if(RowHeight == 0)
                    RowHeight = 25;

                this.ClientSettings.Scrolling.ScrollHeight = RowHeight * ScreenRowNumber;
            }
            else
            {
                this.ClientSettings.Scrolling.AllowScroll = false;                
            }
        }
    }
}
