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

using System;
using System.Web.UI.WebControls;
using DotNetNuke.Framework.JavaScriptLibraries;

#region Usings

#endregion

#region Usings

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
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