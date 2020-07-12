// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// The AutoCompleteControl is the same as a TextEditControl but it looks up similar values
    /// in the profile of other users in the same portal and offers those in a dropdown under the text
    /// box where the user is entering the value. So if you'd use this as the input control
    /// for "City" it will find all entered cities and look up values as the user types the city in
    /// the textbox. Selection is not enforced and if a user enters a new city it is added to the list.
    /// </summary>
    [ToolboxData("<{0}:TextEditControl runat=server></{0}:TextEditControl>")]
    internal class AutoCompleteControl : TextEditControl
    {
        public AutoCompleteControl()
        {
            this.Init += this.AutoCompleteControl_Init;
            this.Load += this.AutoCompleteControl_Load;
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            int length = Null.NullInteger;
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute)
                    {
                        var lengthAtt = (MaxLengthAttribute)attribute;
                        length = lengthAtt.Length;
                        break;
                    }
                }
            }

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.StringValue);
            if (length > Null.NullInteger)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString());
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute("data-name", this.Name);
            writer.AddAttribute("data-pid", Entities.Portals.PortalSettings.Current.PortalId.ToString());
            writer.AddAttribute("data-editor", "AutoCompleteControl");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private void AutoCompleteControl_Init(object sender, System.EventArgs e)
        {
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/ProfileAutoComplete/dnn.ProfileAutoComplete.js");
            ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/ProfileAutoComplete/dnn.AutoComplete.css");
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
        }

        private void AutoCompleteControl_Load(object sender, System.EventArgs e)
        {
        }
    }
}
