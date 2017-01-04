#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Web.UI.WebControls;

#region Usings

using System;
using DotNetNuke.Framework;

#endregion

#region Usings

using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGrid : GridView
    {

        #region public properties

        public int ScreenRowNumber { get; set; }

        public int RowHeight { get; set; }

        #endregion
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //base.EnableEmbeddedBaseStylesheet = false;
            Utilities.ApplySkin(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            //if (string.IsNullOrEmpty(ClientSettings.ClientEvents.OnGridCreated))
            //{
            //    ClientSettings.ClientEvents.OnGridCreated = "$.dnnGridCreated";
            //}

            this.PreRender += new EventHandler(DnnGrid_PreRender);

            //this.MasterTableView.NoMasterRecordsText = Localization.GetString("NoRecords", Localization.SharedResourceFile);
        }

        void DnnGrid_PreRender(object sender, EventArgs e)
        {
            //var items = this.MasterTableView.Items;
            //if (ScreenRowNumber == 0)
            //    ScreenRowNumber = 15;

            //if (items.Count > ScreenRowNumber)
            //{
            //    // need scroll
            //    this.ClientSettings.Scrolling.AllowScroll = true;
            //    this.ClientSettings.Scrolling.UseStaticHeaders = true;

            //    if(RowHeight == 0)
            //        RowHeight = 25;

            //    this.ClientSettings.Scrolling.ScrollHeight = RowHeight * ScreenRowNumber;
            //}
            //else
            //{
            //    this.ClientSettings.Scrolling.AllowScroll = false;                
            //}
        }
    }
}