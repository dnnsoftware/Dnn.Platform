#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using DotNetNuke.UI.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Web.UI.WebControls.Extensions;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{

    public class DnnTimeZoneEditControl : TextEditControl
    {


        private DnnTimeZoneComboBox TimeZones;
        #region "Constructors"

        public DnnTimeZoneEditControl()
        {
        }

	    public override string EditControlClientId
	    {
		    get
		    {
			    EnsureChildControls();
			    return TimeZones.ClientID;
		    }
	    }

	    #endregion

        protected override void CreateChildControls()
        {
            TimeZones = new DnnTimeZoneComboBox();
            TimeZones.ViewStateMode = ViewStateMode.Disabled;

            Controls.Clear();
            Controls.Add(TimeZones);

            base.CreateChildControls();
        }

        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = StringValue;
            string postedValue = TimeZones.SelectedValue;
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(Name);
            args.Value = TimeZoneInfo.FindSystemTimeZoneById(StringValue);
            args.OldValue = OldStringValue;
            args.StringValue = StringValue;
            base.OnValueChanged(args);
        }

        protected override void OnInit(System.EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            TimeZones.DataBind(StringValue);

            if ((Page != null) && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(System.Web.UI.HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        protected override void RenderViewMode(System.Web.UI.HtmlTextWriter writer)
        {
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value));
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
        }

    }

}
