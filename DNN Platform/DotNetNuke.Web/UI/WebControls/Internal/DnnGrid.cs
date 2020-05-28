// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI.WebControls;
using DotNetNuke.Framework.JavaScriptLibraries;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnGrid : GridView
    {
        #region public properties

        public int ScreenRowNumber { get; set; }

        public int RowHeight { get; set; }

        public int CurrentPageIndex
        {
            get
            {
                return PageIndex;
                
            }
            set { PageIndex = value; }
        }

        public TableItemStyle ItemStyle => RowStyle;
        public TableItemStyle AlternatingItemStyle => AlternatingRowStyle;
        public TableItemStyle EditItemStyle => EditRowStyle;
        public TableItemStyle SelectedItemStyle => SelectedRowStyle;


        #endregion
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

            AlternatingRowStyle.CssClass = "alter-row";
            Style.Remove("border-collapse");
        }
    }
}
