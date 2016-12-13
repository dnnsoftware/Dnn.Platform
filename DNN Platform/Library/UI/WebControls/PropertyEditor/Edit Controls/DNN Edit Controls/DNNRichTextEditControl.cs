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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Modules.HTMLEditorProvider;

#endregion

namespace DotNetNuke.UI.WebControls
{

    /// <summary>
    /// The DNNRichTextEditControl control provides a standard UI component for editing
    /// RichText
    /// </summary>
    /// <remarks>This is still being used in 6.0.0 (edit a profile, biography field).</remarks>
    [Obsolete("Deprecated in DNN 5.6.2.  Replaced by control of same name in DotNetNuke.Web assembly")]
    [ToolboxData("<{0}:DNNRichTextEditControl runat=server></{0}:DNNRichTextEditControl>")]
    public class DNNRichTextEditControl : TextEditControl
    {
        private HtmlEditorProvider _richTextEditor;
        private TextBox _defaultTextEditor;

        protected Control TextEditControl
        {
            get
            {
                if (_richTextEditor != null)
                {
                    return _richTextEditor.HtmlEditorControl;
                }

                return _defaultTextEditor;
            }
        }

        protected string EditorText
        {
            get
            {
                if (_richTextEditor != null)
                {
                    return _richTextEditor.Text;
                }

                return _defaultTextEditor.Text;
            }
            set
            {
                if (_richTextEditor != null)
                {
                    _richTextEditor.Text = value;
                }
                else
                {
                    _defaultTextEditor.Text = value;
                }
            }
        }

        protected override void CreateChildControls()
        {
            if (EditMode == PropertyEditorMode.Edit)
            {
                var pnlEditor = new Panel();
                if(string.IsNullOrEmpty(CssClass))
                {
                    pnlEditor.CssClass ="dnnLeft";
                }
                else
                {
                    pnlEditor.CssClass = string.Format("{0} dnnLeft", CssClass);
                }


                _richTextEditor = HtmlEditorProvider.Instance();
                if (_richTextEditor != null)
                {
                    _richTextEditor.ControlID = ID + "edit";
                    _richTextEditor.Initialize();
                    _richTextEditor.Height = ControlStyle.Height;
                    _richTextEditor.Width = ControlStyle.Width;
                    if (_richTextEditor.Height.IsEmpty)
                    {
                        _richTextEditor.Height = new Unit(250);
                    }

                    _richTextEditor.Width = new Unit(400);
                }
                else
                {
                    _defaultTextEditor = new TextBox
                                         {
                                             ID = ID + "edit",
                                             Width = ControlStyle.Width.IsEmpty ? new Unit(300) : ControlStyle.Width,
                                             Height = ControlStyle.Height.IsEmpty ? new Unit(250) : ControlStyle.Height,
                                             TextMode = TextBoxMode.MultiLine
                                         };
                }

                Controls.Clear();
                pnlEditor.Controls.Add(TextEditControl);
                Controls.Add(pnlEditor);
                
            }
            base.CreateChildControls();
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var dataChanged = false;
            var presentValue = StringValue;
            var postedValue = EditorText;
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }

        protected override void OnDataChanged(EventArgs e)
        {
            var strValue = RemoveBaseTags(Convert.ToString(Value));
            var strOldValue = RemoveBaseTags(Convert.ToString(OldValue));
            var args = new PropertyEditorEventArgs(Name) { Value = Page.Server.HtmlEncode(strValue), OldValue = Page.Server.HtmlEncode(strOldValue), StringValue = Page.Server.HtmlEncode(RemoveBaseTags(StringValue)) };
            base.OnValueChanged(args);
        }

        private string RemoveBaseTags(String strInput)
        {
            return Globals.BaseTagRegex.Replace(strInput, " ");
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (EditMode == PropertyEditorMode.Edit)
            {
                EditorText = Page.Server.HtmlDecode(Convert.ToString(Value));
            }
            if (Page != null && EditMode == PropertyEditorMode.Edit)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            RenderChildren(writer);
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = Page.Server.HtmlDecode(Convert.ToString(Value));
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
        }

    }
}
