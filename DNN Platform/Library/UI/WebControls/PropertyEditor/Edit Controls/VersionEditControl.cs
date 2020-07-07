// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      VersionEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The VersionEditControl control provides a standard UI component for editing
    /// System.Version properties.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:VersionEditControl runat=server></{0}:VersionEditControl>")]
    public class VersionEditControl : EditControl
    {
        protected Version Version
        {
            get
            {
                return this.Value as Version;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets stringValue is the value of the control expressed as a String.
        /// </summary>
        /// <value>A string representing the Value.</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                return this.Value.ToString();
            }

            set
            {
                this.Value = new Version(value);
            }
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string majorVersion = postCollection[postDataKey + "_Major"];
            string minorVersion = postCollection[postDataKey + "_Minor"];
            string buildVersion = postCollection[postDataKey + "_Build"];
            bool dataChanged = false;
            Version presentValue = this.Version;
            var postedValue = new Version(majorVersion + "." + minorVersion + "." + buildVersion);
            if (!postedValue.Equals(presentValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }

            return dataChanged;
        }

        protected void RenderDropDownList(HtmlTextWriter writer, string type, int val)
        {
            // Render the Select Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_" + type);
            writer.AddStyleAttribute("width", "60px");
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            for (int i = 0; i <= 99; i++)
            {
                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, i.ToString());
                if (val == i)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(i.ToString("00"));
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.Value;
            args.OldValue = this.OldValue;
            args.StringValue = this.StringValue;

            this.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs just before the control is rendered.  It forces a postback to the
        /// Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            // Render a containing span Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            // Render Major
            this.RenderDropDownList(writer, "Major", this.Version.Major);

            writer.Write("&nbsp;");

            // Render Minor
            this.RenderDropDownList(writer, "Minor", this.Version.Minor);

            writer.Write("&nbsp;");

            // Render Build
            this.RenderDropDownList(writer, "Build", this.Version.Build);

            // Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (this.Version != null)
            {
                writer.Write(this.Version.ToString(3));
            }

            writer.RenderEndTag();
        }
    }
}
