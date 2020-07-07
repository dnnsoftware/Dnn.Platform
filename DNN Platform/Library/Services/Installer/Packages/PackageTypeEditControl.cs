// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages.WebControls
{
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.WebControls;

    using DNNLocalization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Services.Installer.Packages.WebControls
    /// Class:      PackageTypeEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageTypeEditControl control provides a standard UI component for editing
    /// package types.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:PackageTypeEditControl runat=server></{0}:PackageTypeEditControl>")]
    public class PackageTypeEditControl : TextEditControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            IList<PackageType> packageTypes = PackageController.Instance.GetExtensionPackageTypes();

            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            // Add the Not Specified Option
            writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);

            if (this.StringValue == Null.NullString)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            // Render Option Tag
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write("<" + DNNLocalization.GetString("Not_Specified", DNNLocalization.SharedResourceFile) + ">");
            writer.RenderEndTag();

            foreach (PackageType type in packageTypes)
            {
                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, type.PackageType);

                if (type.PackageType == this.StringValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(type.PackageType);
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
        }
    }
}
