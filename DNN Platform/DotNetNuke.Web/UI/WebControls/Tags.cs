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


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class Tags : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
        private string _RepeatDirection = "Horizontal";
        private string _Separator = ",&nbsp;";

        private string _Tags;

        private Vocabulary TagVocabulary
        {
            get
            {
                VocabularyController vocabularyController = new VocabularyController();
                return (from v in vocabularyController.GetVocabularies() where v.IsSystem && v.Name == "Tags" select v).SingleOrDefault();
            }
        }

        #region "Public Properties"

        public string AddImageUrl { get; set; }

        public bool AllowTagging { get; set; }

        public string CancelImageUrl { get; set; }

        public ContentItem ContentItem { get; set; }

        public bool IsEditMode
        {
            get
            {
                bool _IsEditMode = false;
                if (ViewState["IsEditMode"] != null)
                {
                    _IsEditMode = Convert.ToBoolean(ViewState["IsEditMode"]);
                }
                return _IsEditMode;
            }
            set
            {
                ViewState["IsEditMode"] = value;
            }
        }

        public string NavigateUrlFormatString { get; set; }

        public string RepeatDirection
        {
            get
            {
                return _RepeatDirection;
            }
            set
            {
                _RepeatDirection = value;
            }
        }

        public string SaveImageUrl { get; set; }

        public string Separator
        {
            get
            {
                return _Separator;
            }
            set
            {
                _Separator = value;
            }
        }

        public bool ShowCategories { get; set; }

        public bool ShowTags { get; set; }

        #endregion

        #region "Private Methods"

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
            writer.AddAttribute(HtmlTextWriterAttribute.Title, LocalizeString(string.Format("{0}.ToolTip", buttonType)));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, buttonType));
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            //Image
            if (!string.IsNullOrEmpty(imageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveUrl(imageUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.Write(LocalizeString(buttonType));
            writer.RenderEndTag();
        }

        private void RenderTerm(HtmlTextWriter writer, Term term, bool renderSeparator)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format(NavigateUrlFormatString, term.Name));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, term.Name);
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "tag");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(term.Name);
            writer.RenderEndTag();

            if (renderSeparator)
            {
                writer.Write(Separator);
            }
        }

        private void SaveTags()
        {
            string tags = _Tags;

            if (!string.IsNullOrEmpty(tags))
            {
                foreach (string t in tags.Split(','))
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        string tagName = t.Trim(' ');
                        Term existingTerm = (from term in ContentItem.Terms.AsQueryable() where term.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase) select term).SingleOrDefault();

                        if (existingTerm == null)
                        {
                            //Not tagged
                            TermController termController = new TermController();
                            Term term =
                                (from te in termController.GetTermsByVocabulary(TagVocabulary.VocabularyId) where te.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase) select te).
                                    SingleOrDefault();
                            if (term == null)
                            {
                                //Add term
                                term = new Term(TagVocabulary.VocabularyId);
                                term.Name = tagName;
                                termController.AddTerm(term);
                            }

                            //Add term to content
                            ContentItem.Terms.Add(term);
                            termController.AddTermToContent(term, ContentItem);
                        }
                    }
                }
            }

            IsEditMode = false;

            //Raise the Tags Updated Event
            OnTagsUpdate(EventArgs.Empty);
        }

        #endregion

        public event EventHandler<EventArgs> TagsUpdated;

        protected void OnTagsUpdate(EventArgs e)
        {
            if (TagsUpdated != null)
            {
                TagsUpdated(this, e);
            }
        }

        #region "Public Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if ((!Page.ClientScript.IsClientScriptBlockRegistered(UniqueID)))
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

                Page.ClientScript.RegisterClientScriptBlock(GetType(), UniqueID, sb.ToString());
            }
        }


        public override void RenderControl(HtmlTextWriter writer)
        {
            //Render Outer Div
            writer.AddAttribute(HtmlTextWriterAttribute.Class, RepeatDirection.ToLower());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //Render Categories
            if (ShowCategories)
            {
                //Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "categories");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, LocalizeString("Category.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                //Render Category Links
                var categories = (from cat in ContentItem.Terms where cat.VocabularyId != TagVocabulary.VocabularyId select cat);

                for (int i = 0; i <= categories.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        //First Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == categories.Count() - 1)
                    {
                        //Last Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    RenderTerm(writer, categories.ToList()[i], i < categories.Count() - 1 && RepeatDirection.ToLower() == "horizontal");

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            if (ShowTags)
            {
                //Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "tags");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, LocalizeString("Tag.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                //Render Tag Links
                var tags = (from cat in ContentItem.Terms where cat.VocabularyId == TagVocabulary.VocabularyId select cat);

                for (int i = 0; i <= tags.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        //First Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == tags.Count() - 1)
                    {
                        //Last Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    RenderTerm(writer, tags.ToList()[i], i < tags.Count() - 1 && RepeatDirection.ToLower() == "horizontal");

                    writer.RenderEndTag();
                }

                if (AllowTagging)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    if (IsEditMode)
                    {
                        writer.Write("&nbsp;&nbsp;");

                        writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
                        writer.AddAttribute("OnKeyPress", "return disableEnterKey(event)");
                        writer.RenderBeginTag(HtmlTextWriterTag.Input);
                        writer.RenderEndTag();

                        writer.Write("&nbsp;&nbsp;");

                        //Render Save Button
                        RenderButton(writer, "Save", SaveImageUrl);

                        writer.Write("&nbsp;&nbsp;");

                        //Render Add Button
                        RenderButton(writer, "Cancel", CancelImageUrl);
                    }
                    else
                    {
                        writer.Write("&nbsp;&nbsp;");

                        //Render Add Button
                        RenderButton(writer, "Add", AddImageUrl);
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        #endregion

        #region "IPostBackDataHandler Implementation"

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            _Tags = postCollection[postDataKey];

            return true;
        }


        public void RaisePostDataChangedEvent()
        {
        }

        #endregion

        #region "IPostBackEventHandler Implementation"

        public void RaisePostBackEvent(string eventArgument)
        {
            switch (eventArgument)
            {
                case "Add":
                    IsEditMode = true;
                    break;
                case "Cancel":
                    IsEditMode = false;
                    break;
                case "Save":
                    SaveTags();
                    break;
                default:
                    IsEditMode = false;
                    break;
            }
        }

        #endregion
    }
}