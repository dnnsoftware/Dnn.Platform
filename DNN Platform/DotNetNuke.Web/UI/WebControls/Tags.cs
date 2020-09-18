// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;

    public class Tags : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
        private string _RepeatDirection = "Horizontal";
        private string _Separator = ",&nbsp;";

        private string _Tags;

        public event EventHandler<EventArgs> TagsUpdated;

        public string AddImageUrl { get; set; }

        public bool AllowTagging { get; set; }

        public string CancelImageUrl { get; set; }

        public ContentItem ContentItem { get; set; }

        public bool IsEditMode
        {
            get
            {
                bool _IsEditMode = false;
                if (this.ViewState["IsEditMode"] != null)
                {
                    _IsEditMode = Convert.ToBoolean(this.ViewState["IsEditMode"]);
                }

                return _IsEditMode;
            }

            set
            {
                this.ViewState["IsEditMode"] = value;
            }
        }

        public string NavigateUrlFormatString { get; set; }

        public string RepeatDirection
        {
            get
            {
                return this._RepeatDirection;
            }

            set
            {
                this._RepeatDirection = value;
            }
        }

        public string SaveImageUrl { get; set; }

        public string Separator
        {
            get
            {
                return this._Separator;
            }

            set
            {
                this._Separator = value;
            }
        }

        public bool ShowCategories { get; set; }

        public bool ShowTags { get; set; }

        private Vocabulary TagVocabulary
        {
            get
            {
                VocabularyController vocabularyController = new VocabularyController();
                return (from v in vocabularyController.GetVocabularies() where v.IsSystem && v.Name == "Tags" select v).SingleOrDefault();
            }
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            // Render Outer Div
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.RepeatDirection.ToLowerInvariant());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Render Categories
            if (this.ShowCategories)
            {
                // Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "categories");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString("Category.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                // Render Category Links
                var categories = from cat in this.ContentItem.Terms where cat.VocabularyId != this.TagVocabulary.VocabularyId select cat;

                for (int i = 0; i <= categories.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        // First Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == categories.Count() - 1)
                    {
                        // Last Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    this.RenderTerm(writer, categories.ToList()[i], i < categories.Count() - 1 && this.RepeatDirection.ToLowerInvariant() == "horizontal");

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            if (this.ShowTags)
            {
                // Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "tags");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString("Tag.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                // Render Tag Links
                var tags = from cat in this.ContentItem.Terms where cat.VocabularyId == this.TagVocabulary.VocabularyId select cat;

                for (int i = 0; i <= tags.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        // First Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == tags.Count() - 1)
                    {
                        // Last Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    this.RenderTerm(writer, tags.ToList()[i], i < tags.Count() - 1 && this.RepeatDirection.ToLowerInvariant() == "horizontal");

                    writer.RenderEndTag();
                }

                if (this.AllowTagging)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    if (this.IsEditMode)
                    {
                        writer.Write("&nbsp;&nbsp;");

                        writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
                        writer.AddAttribute("OnKeyPress", "return disableEnterKey(event)");
                        writer.RenderBeginTag(HtmlTextWriterTag.Input);
                        writer.RenderEndTag();

                        writer.Write("&nbsp;&nbsp;");

                        // Render Save Button
                        this.RenderButton(writer, "Save", this.SaveImageUrl);

                        writer.Write("&nbsp;&nbsp;");

                        // Render Add Button
                        this.RenderButton(writer, "Cancel", this.CancelImageUrl);
                    }
                    else
                    {
                        writer.Write("&nbsp;&nbsp;");

                        // Render Add Button
                        this.RenderButton(writer, "Add", this.AddImageUrl);
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            this._Tags = postCollection[postDataKey];

            return true;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            switch (eventArgument)
            {
                case "Add":
                    this.IsEditMode = true;
                    break;
                case "Cancel":
                    this.IsEditMode = false;
                    break;
                case "Save":
                    this.SaveTags();
                    break;
                default:
                    this.IsEditMode = false;
                    break;
            }
        }

        protected void OnTagsUpdate(EventArgs e)
        {
            if (this.TagsUpdated != null)
            {
                this.TagsUpdated(this, e);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!this.Page.ClientScript.IsClientScriptBlockRegistered(this.UniqueID))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("<script language='javascript' type='text/javascript' >");
                sb.Append(Environment.NewLine);
                sb.Append("function disableEnterKey(e)");
                sb.Append("{");
                sb.Append("var key;");
                sb.Append("if(window.event)");
                sb.Append("key = window.event.keyCode;");
                sb.Append("else ");
                sb.Append("key = e.which;");
                sb.Append("return (key != 13);");
                sb.Append("}");
                sb.Append("</script>");

                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), this.UniqueID, sb.ToString());
            }
        }

        private string LocalizeString(string key)
        {
            string LocalResourceFile = Utilities.GetLocalResourceFile(this);
            string localizedString = null;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(LocalResourceFile))
            {
                localizedString = Localization.GetString(key, LocalResourceFile);
            }
            else
            {
                localizedString = Null.NullString;
            }

            return localizedString;
        }

        private void RenderButton(HtmlTextWriter writer, string buttonType, string imageUrl)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString(string.Format("{0}.ToolTip", buttonType)));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, buttonType));
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            // Image
            if (!string.IsNullOrEmpty(imageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveUrl(imageUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.Write(this.LocalizeString(buttonType));
            writer.RenderEndTag();
        }

        private void RenderTerm(HtmlTextWriter writer, Term term, bool renderSeparator)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format(this.NavigateUrlFormatString, term.Name));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, term.Name);
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "tag");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(term.Name);
            writer.RenderEndTag();

            if (renderSeparator)
            {
                writer.Write(this.Separator);
            }
        }

        private void SaveTags()
        {
            string tags = this._Tags;

            if (!string.IsNullOrEmpty(tags))
            {
                foreach (string t in tags.Split(','))
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        string tagName = t.Trim(' ');
                        Term existingTerm = (from term in this.ContentItem.Terms.AsQueryable() where term.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase) select term).SingleOrDefault();

                        if (existingTerm == null)
                        {
                            // Not tagged
                            TermController termController = new TermController();
                            Term term =
                                (from te in termController.GetTermsByVocabulary(this.TagVocabulary.VocabularyId) where te.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase) select te).
                                    SingleOrDefault();
                            if (term == null)
                            {
                                // Add term
                                term = new Term(this.TagVocabulary.VocabularyId);
                                term.Name = tagName;
                                termController.AddTerm(term);
                            }

                            // Add term to content
                            this.ContentItem.Terms.Add(term);
                            termController.AddTermToContent(term, this.ContentItem);
                        }
                    }
                }
            }

            this.IsEditMode = false;

            // Raise the Tags Updated Event
            this.OnTagsUpdate(EventArgs.Empty);
        }
    }
}
