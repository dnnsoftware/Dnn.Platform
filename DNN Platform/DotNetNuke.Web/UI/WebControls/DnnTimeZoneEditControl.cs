// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    public class DnnTimeZoneEditControl : TextEditControl
    {
        private DnnTimeZoneComboBox TimeZones;

        public DnnTimeZoneEditControl()
        {
        }

        public override string EditControlClientId
        {
            get
            {
                this.EnsureChildControls();
                return this.TimeZones.ClientID;
            }
        }

        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = this.TimeZones.SelectedValue;
            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        protected override void CreateChildControls()
        {
            this.TimeZones = new DnnTimeZoneComboBox();
            this.TimeZones.ViewStateMode = ViewStateMode.Disabled;

            this.Controls.Clear();
            this.Controls.Add(this.TimeZones);

            base.CreateChildControls();
        }

        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = TimeZoneInfo.FindSystemTimeZoneById(this.StringValue);
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
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

            this.TimeZones.DataBind(this.StringValue);

            if ((this.Page != null) && this.EditMode == PropertyEditorMode.Edit)
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
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
        }
    }
}
