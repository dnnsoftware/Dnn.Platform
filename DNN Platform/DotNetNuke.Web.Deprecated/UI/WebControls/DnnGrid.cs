// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using Telerik.Web.UI;

    public class DnnGrid : RadGrid
    {
        public int ScreenRowNumber { get; set; }

        public int RowHeight { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnableEmbeddedBaseStylesheet = false;
            Utilities.ApplySkin(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            if (string.IsNullOrEmpty(this.ClientSettings.ClientEvents.OnGridCreated))
            {
                this.ClientSettings.ClientEvents.OnGridCreated = "$.dnnGridCreated";
            }

            this.PreRender += new EventHandler(this.DnnGrid_PreRender);

            this.MasterTableView.NoMasterRecordsText = Localization.GetString("NoRecords", Localization.SharedResourceFile);
        }

        private void DnnGrid_PreRender(object sender, EventArgs e)
        {
            var items = this.MasterTableView.Items;
            if (this.ScreenRowNumber == 0)
            {
                this.ScreenRowNumber = 15;
            }

            if (items.Count > this.ScreenRowNumber)
            {
                // need scroll
                this.ClientSettings.Scrolling.AllowScroll = true;
                this.ClientSettings.Scrolling.UseStaticHeaders = true;

                if (this.RowHeight == 0)
                {
                    this.RowHeight = 25;
                }

                this.ClientSettings.Scrolling.ScrollHeight = this.RowHeight * this.ScreenRowNumber;
            }
            else
            {
                this.ClientSettings.Scrolling.AllowScroll = false;
            }
        }
    }
}
