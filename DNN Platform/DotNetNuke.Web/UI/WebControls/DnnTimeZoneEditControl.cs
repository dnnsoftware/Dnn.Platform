// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    public class DnnTimeZoneEditControl : TextEditControl
    {
        private DnnTimeZoneComboBox timeZones;

        /// <summary>Initializes a new instance of the <see cref="DnnTimeZoneEditControl"/> class.</summary>
        public DnnTimeZoneEditControl()
        {
        }

        /// <inheritdoc/>
        public override string EditControlClientId
        {
            get
            {
                this.EnsureChildControls();
                return this.timeZones.ClientID;
            }
        }

        /// <inheritdoc/>
        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = this.StringValue;
            string postedValue = this.timeZones.SelectedValue;
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            this.timeZones = new DnnTimeZoneComboBox();
            this.timeZones.ViewStateMode = ViewStateMode.Disabled;

            this.Controls.Clear();
            this.Controls.Add(this.timeZones);

            base.CreateChildControls();
        }

        /// <inheritdoc/>
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = TimeZoneInfo.FindSystemTimeZoneById(this.StringValue);
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
        }

        /// <inheritdoc/>
        protected override void OnInit(System.EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        /// <inheritdoc/>
        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            this.timeZones.DataBind(this.StringValue);

            if ((this.Page != null) && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// <inheritdoc/>
        protected override void RenderEditMode(System.Web.UI.HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        /// <inheritdoc/>
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
