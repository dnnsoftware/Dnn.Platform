// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Specialized;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
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
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                return Value.ToString();
            }
            set
            {
                Value = new Version(value);
            }
        }

        protected Version Version
        {
            get
            {
                return Value as Version;
            }
        }
		
		#endregion

        protected void RenderDropDownList(HtmlTextWriter writer, string type, int val)
        {
            //Render the Select Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "_" + type);
            writer.AddStyleAttribute("width", "60px");
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            for (int i = 0; i <= 99; i++)
            {
                //Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, i.ToString());
                if (val == i)
                {
                    //Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
				
                //Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(i.ToString("00"));
                writer.RenderEndTag();
            }
			
            //Close Select Tag
            writer.RenderEndTag();
        }
		
		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(Name);
            args.Value = Value;
            args.OldValue = OldValue;
            args.StringValue = StringValue;

            base.OnValueChanged(args);
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

            if (Page != null && EditMode == PropertyEditorMode.Edit)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            //Render a containing span Tag
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            //Render Major
            RenderDropDownList(writer, "Major", Version.Major);

            writer.Write("&nbsp;");

            //Render Minor
            RenderDropDownList(writer, "Minor", Version.Minor);

            writer.Write("&nbsp;");

            //Render Build
            RenderDropDownList(writer, "Build", Version.Build);

            //Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (Version != null)
            {
                writer.Write(Version.ToString(3));
            }
            writer.RenderEndTag();
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string majorVersion = postCollection[postDataKey + "_Major"];
            string minorVersion = postCollection[postDataKey + "_Minor"];
            string buildVersion = postCollection[postDataKey + "_Build"];
            bool dataChanged = false;
            Version presentValue = Version;
            var postedValue = new Version(majorVersion + "." + minorVersion + "." + buildVersion);
            if (!postedValue.Equals(presentValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }
		
		#endregion
    }
}
