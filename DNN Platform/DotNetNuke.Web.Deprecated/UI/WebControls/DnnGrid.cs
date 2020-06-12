// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            if (string.IsNullOrEmpty(this.ClientSettings.ClientEvents.OnGridCreated))
            {
                this.ClientSettings.ClientEvents.OnGridCreated = "$.dnnGridCreated";
            }

            this.PreRender += new EventHandler(this.DnnGrid_PreRender);

            this.MasterTableView.NoMasterRecordsText = Localization.GetString("NoRecords", Localization.SharedResourceFile);
        }

        void DnnGrid_PreRender(object sender, EventArgs e)
        {
            var items = this.MasterTableView.Items;
            if (this.ScreenRowNumber == 0)
                this.ScreenRowNumber = 15;

            if (items.Count > this.ScreenRowNumber)
            {
                // need scroll
                this.ClientSettings.Scrolling.AllowScroll = true;
                this.ClientSettings.Scrolling.UseStaticHeaders = true;

                if (this.RowHeight == 0)
                    this.RowHeight = 25;

                this.ClientSettings.Scrolling.ScrollHeight = this.RowHeight * this.ScreenRowNumber;
            }
            else
            {
                this.ClientSettings.Scrolling.AllowScroll = false;                
            }
        }
    }
}
