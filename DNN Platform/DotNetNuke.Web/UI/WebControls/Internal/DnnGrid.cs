// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework.JavaScriptLibraries;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnGrid : GridView
    {
        public TableItemStyle ItemStyle => this.RowStyle;

        public TableItemStyle AlternatingItemStyle => this.AlternatingRowStyle;

        public TableItemStyle EditItemStyle => this.EditRowStyle;

        public TableItemStyle SelectedItemStyle => this.SelectedRowStyle;
        public int ScreenRowNumber { get; set; }

        public int RowHeight { get; set; }

        public int CurrentPageIndex
        {
            get
            {
                return this.PageIndex;
            }

            set { this.PageIndex = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.CssClass = "dnn-grid";
            Utilities.ApplySkin(this);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.AlternatingRowStyle.CssClass = "alter-row";
            this.Style.Remove("border-collapse");
        }
    }
}
