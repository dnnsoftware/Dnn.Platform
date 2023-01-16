﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Services.Localization;

    [ToolboxData("<{0}:PagingControl runat=server></{0}:PagingControl>")]
    public class PagingControl : WebControl, IPostBackEventHandler
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Repeater PageNumbers;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TableCell cellDisplayLinks;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TableCell cellDisplayStatus;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected Table tablePageNumbers;
        private int totalPages = -1;
        private string cssClassLinkActive;
        private string cssClassLinkInactive;
        private string cssClassPagingStatus;
        private PagingControlMode mode = PagingControlMode.URL;

        public event EventHandler PageChanged;

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassLinkActive
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassLinkActive) ? string.Empty : this.cssClassLinkActive;
            }

            set
            {
                this.cssClassLinkActive = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassLinkInactive
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassLinkInactive) ? string.Empty : this.cssClassLinkInactive;
            }

            set
            {
                this.cssClassLinkInactive = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassPagingStatus
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassPagingStatus) ? string.Empty : this.cssClassPagingStatus;
            }

            set
            {
                this.cssClassPagingStatus = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("1")]
        public int CurrentPage { get; set; }

        public PagingControlMode Mode
        {
            get
            {
                return this.mode;
            }

            set
            {
                this.mode = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("10")]
        public int PageSize { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string QuerystringParams { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("-1")]
        public int TabID { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("0")]
        public int TotalRecords { get; set; }

        /// <inheritdoc/>
        public void RaisePostBackEvent(string eventArgument)
        {
            this.CurrentPage = int.Parse(eventArgument.Replace("Page_", string.Empty));
            this.OnPageChanged(new EventArgs());
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            this.tablePageNumbers = new Table();
            this.cellDisplayStatus = new TableCell();
            this.cellDisplayLinks = new TableCell();

            // cellDisplayStatus.CssClass = "Normal";
            // cellDisplayLinks.CssClass = "Normal";
            this.tablePageNumbers.CssClass = string.IsNullOrEmpty(this.CssClass) ? "PagingTable" : this.CssClass;
            var intRowIndex = this.tablePageNumbers.Rows.Add(new TableRow());
            this.PageNumbers = new Repeater();
            var i = new PageNumberLinkTemplate(this);
            this.PageNumbers.ItemTemplate = i;
            this.BindPageNumbers(this.TotalRecords, this.PageSize);
            this.cellDisplayStatus.HorizontalAlign = HorizontalAlign.Left;

            // cellDisplayStatus.Width = new Unit("50%");
            this.cellDisplayLinks.HorizontalAlign = HorizontalAlign.Right;

            // cellDisplayLinks.Width = new Unit("50%");
            var intTotalPages = this.totalPages;
            if (intTotalPages == 0)
            {
                intTotalPages = 1;
            }

            var str = string.Format(Localization.GetString("Pages"), this.CurrentPage, intTotalPages);
            var lit = new LiteralControl(str);
            this.cellDisplayStatus.Controls.Add(lit);
            this.tablePageNumbers.Rows[intRowIndex].Cells.Add(this.cellDisplayStatus);
            this.tablePageNumbers.Rows[intRowIndex].Cells.Add(this.cellDisplayLinks);
        }

        protected void OnPageChanged(EventArgs e)
        {
            if (this.PageChanged != null)
            {
                this.PageChanged(this, e);
            }
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter output)
        {
            if (this.PageNumbers == null)
            {
                this.CreateChildControls();
            }

            var str = new StringBuilder();
            str.Append(this.GetFirstLink() + "&nbsp;&nbsp;&nbsp;");
            str.Append(this.GetPreviousLink() + "&nbsp;&nbsp;&nbsp;");
            var result = new StringBuilder(1024);
            this.PageNumbers.RenderControl(new HtmlTextWriter(new StringWriter(result)));
            str.Append(result.ToString());
            str.Append(this.GetNextLink() + "&nbsp;&nbsp;&nbsp;");
            str.Append(this.GetLastLink() + "&nbsp;&nbsp;&nbsp;");
            this.cellDisplayLinks.Controls.Add(new LiteralControl(str.ToString()));
            this.tablePageNumbers.RenderControl(output);
        }

        private void BindPageNumbers(int totalRecords, int recordsPerPage)
        {
            const int pageLinksPerPage = 10;
            if (totalRecords < 1 || recordsPerPage < 1)
            {
                this.totalPages = 1;
                return;
            }

            this.totalPages = totalRecords / recordsPerPage >= 1 ? Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalRecords) / recordsPerPage)) : 0;
            if (this.totalPages > 0)
            {
                var ht = new DataTable();
                ht.Columns.Add("PageNum");
                DataRow tmpRow;
                var lowNum = 1;
                var highNum = Convert.ToInt32(this.totalPages);
                double tmpNum;
                tmpNum = this.CurrentPage - (pageLinksPerPage / 2);
                if (tmpNum < 1)
                {
                    tmpNum = 1;
                }

                if (this.CurrentPage > (pageLinksPerPage / 2))
                {
                    lowNum = Convert.ToInt32(Math.Floor(tmpNum));
                }

                if (Convert.ToInt32(this.totalPages) <= pageLinksPerPage)
                {
                    highNum = Convert.ToInt32(this.totalPages);
                }
                else
                {
                    highNum = lowNum + pageLinksPerPage - 1;
                }

                if (highNum > Convert.ToInt32(this.totalPages))
                {
                    highNum = Convert.ToInt32(this.totalPages);
                    if (highNum - lowNum < pageLinksPerPage)
                    {
                        lowNum = highNum - pageLinksPerPage + 1;
                    }
                }

                if (highNum > Convert.ToInt32(this.totalPages))
                {
                    highNum = Convert.ToInt32(this.totalPages);
                }

                if (lowNum < 1)
                {
                    lowNum = 1;
                }

                int i;
                for (i = lowNum; i <= highNum; i++)
                {
                    tmpRow = ht.NewRow();
                    tmpRow["PageNum"] = i;
                    ht.Rows.Add(tmpRow);
                }

                this.PageNumbers.DataSource = ht;
                this.PageNumbers.DataBind();
            }
        }

        private string CreateURL(string currentPage)
        {
            switch (this.Mode)
            {
                case PagingControlMode.URL:
                    return !string.IsNullOrEmpty(this.QuerystringParams)
                               ? (!string.IsNullOrEmpty(currentPage) ? TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, this.QuerystringParams, "currentpage=" + currentPage) : TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, this.QuerystringParams))
                               : (!string.IsNullOrEmpty(currentPage) ? TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, "currentpage=" + currentPage) : TestableGlobals.Instance.NavigateURL(this.TabID));
                default:
                    return this.Page.ClientScript.GetPostBackClientHyperlink(this, "Page_" + currentPage, false);
            }
        }

        /// <summary>GetLink returns the page number links for paging.</summary>
        private string GetLink(int pageNum)
        {
            if (pageNum == this.CurrentPage)
            {
                return this.CSSClassLinkInactive.Trim().Length > 0 ? "<span class=\"" + this.CSSClassLinkInactive + "\">[" + pageNum + "]</span>" : "<span>[" + pageNum + "]</span>";
            }

            return this.CSSClassLinkActive.Trim().Length > 0
                       ? "<a href=\"" + this.CreateURL(pageNum.ToString()) + "\" class=\"" + this.CSSClassLinkActive + "\">" + pageNum + "</a>"
                       : "<a href=\"" + this.CreateURL(pageNum.ToString()) + "\">" + pageNum + "</a>";
        }

        /// <summary>GetPreviousLink returns the link for the Previous page for paging.</summary>
        private string GetPreviousLink()
        {
            return this.CurrentPage > 1 && this.totalPages > 0
                       ? (this.CSSClassLinkActive.Trim().Length > 0
                              ? "<a href=\"" + this.CreateURL((this.CurrentPage - 1).ToString()) + "\" class=\"" + this.CSSClassLinkActive + "\">" +
                                Localization.GetString("Previous", Localization.SharedResourceFile) + "</a>"
                              : "<a href=\"" + this.CreateURL((this.CurrentPage - 1).ToString()) + "\">" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</a>")
                       : (this.CSSClassLinkInactive.Trim().Length > 0
                              ? "<span class=\"" + this.CSSClassLinkInactive + "\">" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</span>"
                              : "<span>" + Localization.GetString("Previous", Localization.SharedResourceFile) + "</span>");
        }

        /// <summary>GetNextLink returns the link for the Next Page for paging.</summary>
        private string GetNextLink()
        {
            return this.CurrentPage != this.totalPages && this.totalPages > 0
                       ? (this.CSSClassLinkActive.Trim().Length > 0
                              ? "<a href=\"" + this.CreateURL((this.CurrentPage + 1).ToString()) + "\" class=\"" + this.CSSClassLinkActive + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) +
                                "</a>"
                              : "<a href=\"" + this.CreateURL((this.CurrentPage + 1).ToString()) + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) + "</a>")
                       : (this.CSSClassLinkInactive.Trim().Length > 0
                              ? "<span class=\"" + this.CSSClassLinkInactive + "\">" + Localization.GetString("Next", Localization.SharedResourceFile) + "</span>"
                              : "<span>" + Localization.GetString("Next", Localization.SharedResourceFile) + "</span>");
        }

        /// <summary>GetFirstLink returns the First Page link for paging.</summary>
        private string GetFirstLink()
        {
            if (this.CurrentPage > 1 && this.totalPages > 0)
            {
                return this.CSSClassLinkActive.Trim().Length > 0
                           ? "<a href=\"" + this.CreateURL("1") + "\" class=\"" + this.CSSClassLinkActive + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</a>"
                           : "<a href=\"" + this.CreateURL("1") + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</a>";
            }

            return this.CSSClassLinkInactive.Trim().Length > 0
                       ? "<span class=\"" + this.CSSClassLinkInactive + "\">" + Localization.GetString("First", Localization.SharedResourceFile) + "</span>"
                       : "<span>" + Localization.GetString("First", Localization.SharedResourceFile) + "</span>";
        }

        /// <summary>GetLastLink returns the Last Page link for paging.</summary>
        private string GetLastLink()
        {
            if (this.CurrentPage != this.totalPages && this.totalPages > 0)
            {
                return this.CSSClassLinkActive.Trim().Length > 0
                           ? "<a href=\"" + this.CreateURL(this.totalPages.ToString()) + "\" class=\"" + this.CSSClassLinkActive + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</a>"
                           : "<a href=\"" + this.CreateURL(this.totalPages.ToString()) + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</a>";
            }

            return this.CSSClassLinkInactive.Trim().Length > 0
                       ? "<span class=\"" + this.CSSClassLinkInactive + "\">" + Localization.GetString("Last", Localization.SharedResourceFile) + "</span>"
                       : "<span>" + Localization.GetString("Last", Localization.SharedResourceFile) + "</span>";
        }

        public class PageNumberLinkTemplate : ITemplate
        {
            private readonly PagingControl pagingControl;

            /// <summary>Initializes a new instance of the <see cref="PageNumberLinkTemplate"/> class.</summary>
            /// <param name="ctlPagingControl"></param>
            public PageNumberLinkTemplate(PagingControl ctlPagingControl)
            {
                this.pagingControl = ctlPagingControl;
            }

            /// <inheritdoc/>
            void ITemplate.InstantiateIn(Control container)
            {
                var l = new Literal();
                l.DataBinding += this.BindData;
                container.Controls.Add(l);
            }

            private void BindData(object sender, EventArgs e)
            {
                Literal lc;
                lc = (Literal)sender;
                RepeaterItem container;
                container = (RepeaterItem)lc.NamingContainer;
                lc.Text = this.pagingControl.GetLink(Convert.ToInt32(DataBinder.Eval(container.DataItem, "PageNum"))) + "&nbsp;&nbsp;";
            }
        }
    }
}
