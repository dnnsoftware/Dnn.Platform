// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Modules.HTMLEditorProvider;

    /// <summary>
    /// The DNNRichTextEditControl control provides a standard UI component for editing
    /// RichText.
    /// </summary>
    [ToolboxData("<{0}:DNNRichTextEditControl runat=server></{0}:DNNRichTextEditControl>")]
    public class DNNRichTextEditControl : TextEditControl
    {
        private HtmlEditorProvider _richTextEditor;
        private TextBox _defaultTextEditor;

        protected Control TextEditControl
        {
            get
            {
                if (this._richTextEditor != null)
                {
                    return this._richTextEditor.HtmlEditorControl;
                }

                return this._defaultTextEditor;
            }
        }

        protected string EditorText
        {
            get
            {
                if (this._richTextEditor != null)
                {
                    return this._richTextEditor.Text;
                }

                return this._defaultTextEditor.Text;
            }

            set
            {
                if (this._richTextEditor != null)
                {
                    this._richTextEditor.Text = value;
                }
                else
                {
                    this._defaultTextEditor.Text = value;
                }
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var dataChanged = false;
            var presentValue = this.StringValue;
            var postedValue = this.EditorText;
            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void CreateChildControls()
        {
            if (this.EditMode == PropertyEditorMode.Edit)
            {
                var pnlEditor = new Panel();
                if (string.IsNullOrEmpty(this.CssClass))
                {
                    pnlEditor.CssClass = "dnnLeft";
                }
                else
                {
                    pnlEditor.CssClass = string.Format("{0} dnnLeft", this.CssClass);
                }

                this._richTextEditor = HtmlEditorProvider.Instance();
                if (this._richTextEditor != null)
                {
                    this._richTextEditor.ControlID = this.ID + "edit";
                    this._richTextEditor.Initialize();
                    this._richTextEditor.Height = this.ControlStyle.Height;
                    this._richTextEditor.Width = this.ControlStyle.Width;
                    if (this._richTextEditor.Height.IsEmpty)
                    {
                        this._richTextEditor.Height = new Unit(250);
                    }

                    this._richTextEditor.Width = new Unit(400);
                }
                else
                {
                    this._defaultTextEditor = new TextBox
                    {
                        ID = this.ID + "edit",
                        Width = this.ControlStyle.Width.IsEmpty ? new Unit(300) : this.ControlStyle.Width,
                        Height = this.ControlStyle.Height.IsEmpty ? new Unit(250) : this.ControlStyle.Height,
                        TextMode = TextBoxMode.MultiLine,
                    };
                    this._defaultTextEditor.Attributes.Add("aria-label", "editor");
                }

                this.Controls.Clear();
                pnlEditor.Controls.Add(this.TextEditControl);
                this.Controls.Add(pnlEditor);
            }

            base.CreateChildControls();
        }

        protected override void OnDataChanged(EventArgs e)
        {
            var strValue = this.RemoveBaseTags(Convert.ToString(this.Value));
            var strOldValue = this.RemoveBaseTags(Convert.ToString(this.OldValue));
            var args = new PropertyEditorEventArgs(this.Name) { Value = this.Page.Server.HtmlEncode(strValue), OldValue = this.Page.Server.HtmlEncode(strOldValue), StringValue = this.Page.Server.HtmlEncode(this.RemoveBaseTags(this.StringValue)) };
            this.OnValueChanged(args);
        }

        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.EditMode == PropertyEditorMode.Edit)
            {
                this.EditorText = this.Page.Server.HtmlDecode(Convert.ToString(this.Value));
            }

            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value));
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
        }

        private string RemoveBaseTags(string strInput)
        {
            return Globals.BaseTagRegex.Replace(strInput, " ");
        }
    }
}
