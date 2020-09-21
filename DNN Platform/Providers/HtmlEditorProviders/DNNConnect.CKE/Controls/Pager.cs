// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Localization;

namespace DNNConnect.CKEditorProvider.Controls
{

    /// <summary>
    /// The html generic self closing.
    /// </summary>
    [ToolboxData("<{0}:Pager runat=server></{0}:Pager>")]
    public class Pager : WebControl, IPostBackEventHandler
    {
        #region Events

        /// <summary>
        /// Occurs when [page changed].
        /// </summary>
        public event EventHandler PageChanged;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets Language Code
        /// </summary>
        public string LanguageCode
        {
            get
            {
                return ViewState["LanguageCode"] != null ? (string)ViewState["LanguageCode"] : "en";
            }

            set
            {
                ViewState["LanguageCode"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets Ressource File
        /// </summary>
        public string RessourceFile
        {
            get
            {
                return (string)ViewState["RessourceFile"];
            }

            set
            {
                ViewState["RessourceFile"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets Page Count.
        /// </summary>
        public int PageCount
        {
            get
            {
                return ViewState["PageCount"] != null ? (int)ViewState["PageCount"] : 0;
            }

            set
            {
                ViewState["PageCount"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets Current Page Index.
        /// </summary>
        public int CurrentPageIndex
        {
            get
            {
                return (int)(ViewState["CurrentPageIndex"] ?? 0);
            }

            set
            {
                ViewState["CurrentPageIndex"] = value;
            }
        }

        #endregion

        #region IPostBackEventHandler Members

        /// <summary>
        /// Enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            CurrentPageIndex = int.Parse(eventArgument.Replace("Page_", string.Empty));
            OnPageChanged(new EventArgs());
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="PageChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnPageChanged(EventArgs e)
        {
            if (PageChanged != null)
            {
                PageChanged(this, e);
            }
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            GeneratePagerLinks(writer);
        }

        /// <summary>
        /// Generates the pager links.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void GeneratePagerLinks(HtmlTextWriter writer)
        {
            var mainTable = new Table { CssClass = "PagerTable" };

            var mainTableRow = new TableRow();

            mainTable.Rows.Add(mainTableRow);

            var previousColumn = new TableCell { CssClass = "PagerFirstColumn" };

            int iStart = CurrentPageIndex - 2;
            int iEnd = CurrentPageIndex + 3;

            if (iStart < 0)
            {
                iStart = 0;
            }

            if (iEnd > PageCount)
            {
                iEnd = PageCount;
            }

            var ulFirstElement = new HtmlGenericControl("ul");

            ulFirstElement.Attributes.Add("class", "FilesPager");

            // First Page
            if (iStart > 0)
            {
                var liFirstElement = new HtmlGenericControl("li");

                liFirstElement.Attributes.Add("class", "FirstPage");

                var firstPageLink = new HyperLink
                {
                    ID = "FirstPageLink",
                    ToolTip = string.Format("{0}{1}", Localization.GetString("GoTo.Text", RessourceFile, LanguageCode), Localization.GetString("FirstPage.Text", RessourceFile, LanguageCode)),
                    Text = string.Format("&laquo; {0}", Localization.GetString("FirstPage.Text", RessourceFile, LanguageCode)),
                    NavigateUrl =
                        Page.ClientScript.GetPostBackClientHyperlink(this, string.Format("Page_{0}", 0), false)
                };

                liFirstElement.Controls.Add(firstPageLink);

                ulFirstElement.Controls.Add(liFirstElement);
            }

            // Previous Page
            if (CurrentPageIndex > iStart)
            {
                var liPrevElement = new HtmlGenericControl("li");

                liPrevElement.Attributes.Add("class", "PreviousPage");

                var lastPrevLink = new HyperLink
                {
                    ID = "PreviousPageLink",
                    ToolTip = string.Format("{0}{1}", Localization.GetString("GoTo.Text", RessourceFile, LanguageCode), Localization.GetString("PreviousPage.Text", RessourceFile, LanguageCode)),
                    Text = string.Format("&lt; {0}", Localization.GetString("PreviousPage.Text", RessourceFile, LanguageCode)),
                    NavigateUrl =
                        Page.ClientScript.GetPostBackClientHyperlink(
                            this, string.Format("Page_{0}", CurrentPageIndex - 1), false)
                };

                liPrevElement.Controls.Add(lastPrevLink);

                ulFirstElement.Controls.Add(liPrevElement);
            }

            // Add Column
            previousColumn.Controls.Add(ulFirstElement);
            mainTableRow.Cells.Add(previousColumn);

            // Second Page Numbers Column
            var ulSecondElement = new HtmlGenericControl("ul");

            var pageNumbersColumn = new TableCell { CssClass = "PagerNumbersColumn", HorizontalAlign = HorizontalAlign.Center };

            ulSecondElement.Attributes.Add("class", "FilesPager");

            for (int i = iStart; i < iEnd; i++)
            {
                var liElement = new HtmlGenericControl("li");

                liElement.Attributes.Add("class", i.Equals(CurrentPageIndex) ? "ActivePage" : "NormalPage");

                var page = (i + 1).ToString();

                var pageLink = new HyperLink
                {
                    ID = string.Format("NextPageLink{0}", page),
                    ToolTip = string.Format("{0}: {1}", Localization.GetString("GoTo.Text", RessourceFile, LanguageCode), page),
                    Text = page,
                    NavigateUrl =
                        Page.ClientScript.GetPostBackClientHyperlink(this, string.Format("Page_{0}", i), false)
                };

                liElement.Controls.Add(pageLink);

                ulSecondElement.Controls.Add(liElement);
            }

            // Add Column
            pageNumbersColumn.Controls.Add(ulSecondElement);
            mainTableRow.Cells.Add(pageNumbersColumn);

            // Last Page Column
            var ulThirdElement = new HtmlGenericControl("ul");

            ulThirdElement.Attributes.Add("class", "FilesPager");

            var lastColumn = new TableCell { CssClass = "PagerLastColumn" };

            // Next Page
            if (CurrentPageIndex < (PageCount - 1))
            {
                var liNextElement = new HtmlGenericControl("li");

                liNextElement.Attributes.Add("class", "NextPage");

                var lastNextLink = new HyperLink
                {
                    ID = "NextPageLink",
                    ToolTip = string.Format("{0}{1}", Localization.GetString("GoTo.Text", RessourceFile, LanguageCode), Localization.GetString("NextPage.Text", RessourceFile, LanguageCode)),
                    Text = string.Format("{0} &gt;", Localization.GetString("NextPage.Text", RessourceFile, LanguageCode)),
                    NavigateUrl =
                        Page.ClientScript.GetPostBackClientHyperlink(
                            this, string.Format("Page_{0}", (CurrentPageIndex + 2 - 1)), false)
                };

                liNextElement.Controls.Add(lastNextLink);

                ulThirdElement.Controls.Add(liNextElement);
            }

            if (iEnd < PageCount)
            {
                var liLastElement = new HtmlGenericControl("li");

                liLastElement.Attributes.Add("class", "LastPage");

                var lastPageLink = new HyperLink
                {
                    ID = "LastPageLink",
                    ToolTip =
                        string.Format(
                            "{0}{1}",
                            Localization.GetString("GoTo.Text", RessourceFile, LanguageCode),
                            Localization.GetString("LastPage.Text", RessourceFile, LanguageCode)),
                    Text =
                        string.Format(
                            "{0} &raquo;",
                            Localization.GetString("LastPage.Text", RessourceFile, LanguageCode)),
                    NavigateUrl =
                        Page.ClientScript.GetPostBackClientHyperlink(
                            this, string.Format("Page_{0}", (PageCount - 1)), false)
                };

                liLastElement.Controls.Add(lastPageLink);

                ulThirdElement.Controls.Add(liLastElement);
            }

            // Add Column
            lastColumn.Controls.Add(ulThirdElement);
            mainTableRow.Cells.Add(lastColumn);

            // Render Complete Control
            mainTable.RenderControl(writer);
        }
    }
}