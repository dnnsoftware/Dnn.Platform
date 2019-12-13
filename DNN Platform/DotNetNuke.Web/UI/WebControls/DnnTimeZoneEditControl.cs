// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
