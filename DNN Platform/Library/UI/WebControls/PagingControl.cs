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
#region Usings

using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Internal;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{

    [ToolboxData("<{0}:PagingControl runat=server></{0}:PagingControl>")]
    public class PagingControl : WebControl, IPostBackEventHandler
    {
        protected Repeater PageNumbers;
        private int _totalPages = -1;
        private string _CSSClassLinkActive;
        private string _CSSClassLinkInactive;
        private string _CSSClassPagingStatus;
        private PagingControlMode _Mode = PagingControlMode.URL;
        protected TableCell cellDisplayLinks;
        protected TableCell cellDisplayStatus;
        protected Table tablePageNumbers;

        [Bindable(true), Category("Behavior"), DefaultValue("")]
        public string CSSClassLinkActive
        {
            get
            {
                return String.IsNullOrEmpty(_CSSClassLinkActive) ? "" : _CSSClassLinkActive;
            }
            set
            {
                _CSSClassLinkActive = value;
            }
        }

        [Bindable(true), Category("Behavior"), DefaultValue("")]
        public string CSSClassLinkInactive
        {
            get
            {
                return String.IsNullOrEmpty(_CSSClassLinkInactive) ? "" : _CSSClassLinkInactive;
            }
            set
            {
                _CSSClassLinkInactive = value;
            }
        }

        [Bindable(true), Category("Behavior"), DefaultValue("")]
        public string CSSClassPagingStatus
        {
            get
            {
                return String.IsNullOrEmpty(_CSSClassPagingStatus) ? "" : _CSSClassPagingStatus;
            }
            set
            {
                _CSSClassPagingStatus = value;
            }
        }

        [Bindable(true), Category("Behavior"), DefaultValue("1")]
        public int CurrentPage { get; set; }

        public PagingControlMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
            }
        }

        [Bindable(true), Category("Behavior"), DefaultValue("10")]
        public int PageSize { get; set; }

        [Bindable(true), Category("Behavior"), DefaultValue("")]
        public string QuerystringParams { get; set; }

        [Bindable(true), Category("Behavior"), DefaultValue("-1")]
        public int TabID { get; set; }

        [Bindable(true), Category("Behavior"), DefaultValue("0")]
        public int TotalRecords { get; set; }

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            CurrentPage = int.Parse(eventArgument.Replace("Page_", ""));
            OnPageChanged(new EventArgs());
        }

        #endregion

        public event EventHandler PageChanged;

        private void BindPageNumbers(int TotalRecords, int RecordsPerPage)
        {
            const int pageLinksPerPage = 10;
            if (TotalRecords < 1 || RecordsPerPage < 1)
            {
                _totalPages = 1;
                return;
            }
            _totalPages = TotalRecords/RecordsPerPage >= 1 ? Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TotalRecords)/RecordsPerPage)) : 0;
            if (_totalPages > 0)
            {
                var ht = new DataTable();
                ht.Columns.Add("PageNum");
                DataRow tmpRow;
                var LowNum = 1;
                var HighNum = Convert.ToInt32(_totalPages);
                double tmpNum;
                tmpNum = CurrentPage - pageLinksPerPage/2;
                if (tmpNum < 1)
                {
                    tmpNum = 1;
                }
                if (CurrentPage > (pageLinksPerPage/2))
                {
                    LowNum = Convert.ToInt32(Math.Floor(tmpNum));
                }
                if (Convert.ToInt32(_totalPages) <= pageLinksPerPage)
                {
                    HighNum = Convert.ToInt32(_totalPages);
                }
                else
                {
                    HighNum = LowNum + pageLinksPerPage - 1;
                }
                if (HighNum > Convert.ToInt32(_totalPages))
                {
                    HighNum = Convert.ToInt32(_totalPages);
                    if (HighNum - LowNum < pageLinksPerPage)
                    {
                        LowNum = HighNum - pageLinksPerPage + 1;
                    }
                }
                if (HighNum > Convert.ToInt32(_totalPages))
                {
                    HighNum = Convert.ToInt32(_totalPages);
                }
                if (LowNum < 1)
                {
                    LowNum = 1;
                }
                int i;
                for (i = LowNum; i <= HighNum; i++)
                {
                    tmpRow = ht.NewRow();
                    tmpRow["PageNum"] = i;
                    ht.Rows.Add(tmpRow);
                }
                PageNumbers.DataSource = ht;
                PageNumbers.DataBind();
            }
        }

        private string CreateURL(string CurrentPage)
        {
            switch (Mode)
            {
                case PagingControlMode.URL:
                    return !String.IsNullOrEmpty(QuerystringParams)
                               ? (!String.IsNullOrEmpty(CurrentPage) ? TestableGlobals.Instance.NavigateURL(TabID, "", QuerystringParams, "currentpage=" + CurrentPage) : TestableGlobals.Instance.NavigateURL(TabID, "", QuerystringParams))
                               : (!String.IsNullOrEmpty(CurrentPage) ? TestableGlobals.Instance.NavigateURL(TabID, "", "currentpage=" + CurrentPage) : TestableGlobals.Instance.NavigateURL(TabID));
                default:
                    return Page.ClientScript.GetPostBackClientHyperlink(this, "Page_" + CurrentPage, false);
            }
        }

        /// <summary>
        /// GetLink returns the page number links for paging.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private string GetLink(int PageNum)
        {
            if (PageNum == CurrentPage)
            {
                return CSSClassLinkInactive.Trim().Length > 0 ? "<span class=\"" + CSSClassLinkInactive + "\">[" + PageNum + "]</span>" : "<span>[" + PageNum + "]</span>";
            }
            return CSSClassLinkActive.Trim().Length > 0
                       ? "<a href=\"" + CreateURL(PageNum.ToString()) + "\" class=\"" + CSSClassLinkActive + "\">" + PageNum + "</a>"
                       : "<a href=\"" + CreateURL(PageNum.ToString()) + "\">" + PageNum + "</a>";
        }

        /// <summary>
        /// GetPreviousLink returns the link for the Previous page for paging.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private string GetPreviousLink()
        {
            return CurrentPage > 1 && _totalPages > 0
                       ? (CSSClassLinkActive.Trim().Length > 0
                              ? "<a href=\"" + CreateURL((CurrentPage - 1).ToString()) + "\" class=\"" + CSSClassLinkActive + "\">" +
                                Localization.GetString("Previous", Localization.SharedResourceFile) + "</a>"
                              : "<a href=\"" + CreateURL((CurrentPage - 1).ToString()) + "\">" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</a>")
                       : (CSSClassLinkInactive.Trim().Length > 0
                              ? "<span class=\"" + CSSClassLinkInactive + "\">" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</span>"
                              : "<span>" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</span>");
        }

        /// <summary>
        /// GetNextLink returns the link for the Next Page for paging.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private string GetNextLink()
        {
            return CurrentPage != _totalPages && _totalPages > 0
                       ? (CSSClassLinkActive.Trim().Length > 0
                              ? "<a href=\"" + CreateURL((CurrentPage + 1).ToString()) + "\" class=\"" + CSSClassLinkActive + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) +
                                "</a>"
                              : "<a href=\"" + CreateURL((CurrentPage + 1).ToString()) + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) + "</a>")
                       : (CSSClassLinkInactive.Trim().Length > 0
                              ? "<span class=\"" + CSSClassLinkInactive + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) + "</span>"
                              : "<span>" + Localization.GetString("Next", Localization.SharedResourceFile) + "</span>");
        }

        /// <summary>
        /// GetFirstLink returns the First Page link for paging.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private string GetFirstLink()
        {
            if (CurrentPage > 1 && _totalPages > 0)
            {
                return CSSClassLinkActive.Trim().Length > 0
                           ? "<a href=\"" + CreateURL("1") + "\" class=\"" + CSSClassLinkActive + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</a>"
                           : "<a href=\"" + CreateURL("1") + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</a>";
            }
            return CSSClassLinkInactive.Trim().Length > 0
                       ? "<span class=\"" + CSSClassLinkInactive + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</span>"
                       : "<span>" + Localization.GetString("First", Localization.SharedResourceFile) + "</span>";
        }

        /// <summary>
        /// GetLastLink returns the Last Page link for paging.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private string GetLastLink()
        {
            if (CurrentPage != _totalPages && _totalPages > 0)
            {
                return CSSClassLinkActive.Trim().Length > 0
                           ? "<a href=\"" + CreateURL(_totalPages.ToString()) + "\" class=\"" + CSSClassLinkActive + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</a>"
                           : "<a href=\"" + CreateURL(_totalPages.ToString()) + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</a>";
            }
            return CSSClassLinkInactive.Trim().Length > 0
                       ? "<span class=\"" + CSSClassLinkInactive + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</span>"
                       : "<span>" + Localization.GetString("Last", Localization.SharedResourceFile) + "</span>";
        }

        protected override void CreateChildControls()
        {
            tablePageNumbers = new Table();
            cellDisplayStatus = new TableCell();
            cellDisplayLinks = new TableCell();
            //cellDisplayStatus.CssClass = "Normal";
            //cellDisplayLinks.CssClass = "Normal";
            tablePageNumbers.CssClass = String.IsNullOrEmpty(CssClass) ? "PagingTable" : CssClass;
            var intRowIndex = tablePageNumbers.Rows.Add(new TableRow());
            PageNumbers = new Repeater();
            var I = new PageNumberLinkTemplate(this);
            PageNumbers.ItemTemplate = I;
            BindPageNumbers(TotalRecords, PageSize);
            cellDisplayStatus.HorizontalAlign = HorizontalAlign.Left;
            //cellDisplayStatus.Width = new Unit("50%");
            cellDisplayLinks.HorizontalAlign = HorizontalAlign.Right;
            //cellDisplayLinks.Width = new Unit("50%");
            var intTotalPages = _totalPages;
            if (intTotalPages == 0)
            {
                intTotalPages = 1;
            }
            var str = string.Format(Localization.GetString("Pages"), CurrentPage, intTotalPages);
            var lit = new LiteralControl(str);
            cellDisplayStatus.Controls.Add(lit);
            tablePageNumbers.Rows[intRowIndex].Cells.Add(cellDisplayStatus);
            tablePageNumbers.Rows[intRowIndex].Cells.Add(cellDisplayLinks);
        }

        protected void OnPageChanged(EventArgs e)
        {
            if (PageChanged != null)
            {
                PageChanged(this, e);
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (PageNumbers == null)
            {
                CreateChildControls();
            }
            var str = new StringBuilder();
            str.Append(GetFirstLink() + "&nbsp;&nbsp;&nbsp;");
            str.Append(GetPreviousLink() + "&nbsp;&nbsp;&nbsp;");
            var result = new StringBuilder(1024);
            PageNumbers.RenderControl(new HtmlTextWriter(new StringWriter(result)));
            str.Append(result.ToString());
            str.Append(GetNextLink() + "&nbsp;&nbsp;&nbsp;");
            str.Append(GetLastLink() + "&nbsp;&nbsp;&nbsp;");
            cellDisplayLinks.Controls.Add(new LiteralControl(str.ToString()));
            tablePageNumbers.RenderControl(output);
        }

        #region Nested type: PageNumberLinkTemplate

        public class PageNumberLinkTemplate : ITemplate
        {
            private readonly PagingControl _PagingControl;

            public PageNumberLinkTemplate(PagingControl ctlPagingControl)
            {
                _PagingControl = ctlPagingControl;
            }

            #region ITemplate Members

            void ITemplate.InstantiateIn(Control container)
            {
                var l = new Literal();
                l.DataBinding += BindData;
                container.Controls.Add(l);
            }

            #endregion

            private void BindData(object sender, EventArgs e)
            {
                Literal lc;
                lc = (Literal) sender;
                RepeaterItem container;
                container = (RepeaterItem) lc.NamingContainer;
                lc.Text = _PagingControl.GetLink(Convert.ToInt32(DataBinder.Eval(container.DataItem, "PageNum"))) + "&nbsp;&nbsp;";
            }
        }

        #endregion
    }
}
