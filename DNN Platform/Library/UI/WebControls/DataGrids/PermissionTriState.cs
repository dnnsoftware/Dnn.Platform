// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls.Internal
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// <summary>
    /// A TriState permission control built specifically for use in the PermissionGrid control
    /// This control is not general in any way shape of form and should NOT be used outside
    /// of the PermissionGrid.
    /// </summary>
    public class PermissionTriState : HiddenField
    {
        private readonly string _grantImagePath;
        private readonly string _denyImagePath;
        private readonly string _nullImagePath;
        private readonly string _lockImagePath;
        private readonly string _grantAltText;
        private readonly string _denyAltText;
        private readonly string _nullAltText;

        public PermissionTriState()
        {
            // kind of ugly to lookup this data each time, but doesn't seem worth the effort to
            // maintain statics for the paths but require a control instance to initialize them
            // and lazy load the text bits when the page instance (or localization) changes
            LookupScriptValues(this, out this._grantImagePath, out this._denyImagePath, out this._nullImagePath, out this._lockImagePath, out this._grantAltText, out this._denyAltText, out this._nullAltText);
        }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        // Locked is currently not used on a post-back and therefore the
        // value on postback is undefined at this time
        public bool Locked { get; set; }

        public string PermissionKey { get; set; }

        public bool SupportsDenyMode { get; set; }

        public static void RegisterScripts(Page page, Control ctl)
        {
            const string scriptKey = "initTriState";
            if (!ClientAPI.IsClientScriptBlockRegistered(page, scriptKey))
            {
                AJAX.RegisterScriptManager();
                JavaScript.RequestRegistration(CommonJs.jQuery);
                ClientAPI.RegisterClientScriptBlock(page, "dnn.permissiontristate.js");

                ClientAPI.RegisterStartUpScript(page, scriptKey, "<script type='text/javascript'>" + GetInitScript(ctl) + "</script>");
            }
        }

        public static string GetInitScript(Control ctl)
        {
            string grantImagePath, denyImagePath, nullImagePath, lockImagePath, grantAltText, denyAltText, nullAltText;

            LookupScriptValues(ctl, out grantImagePath, out denyImagePath, out nullImagePath, out lockImagePath, out grantAltText, out denyAltText, out nullAltText);

            string script =
                    string.Format(
                        @"jQuery(document).ready(
                            function() {{
                                var images = {{ 'True': '{0}', 'False': '{1}', 'Null': '{2}' }};
                                var toolTips = {{ 'True': '{3}', 'False': '{4}', 'Null': '{5}' }};
                                var tsm = dnn.controls.triStateManager(images, toolTips);
                                jQuery('.tristate').each( function(i, elem) {{
                                  tsm.initControl( elem );
                                }});
                             }});",
                        grantImagePath,
                        denyImagePath,
                        nullImagePath,
                        grantAltText,
                        denyAltText,
                        nullAltText);

            return script;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RegisterScripts(this.Page, this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string imagePath;
            string altText;
            switch (this.Value)
            {
                case "True":
                    imagePath = this._grantImagePath;
                    altText = this._grantAltText;
                    break;

                case "False":
                    imagePath = this._denyImagePath;
                    altText = this._denyAltText;
                    break;

                default:
                    imagePath = this._nullImagePath;
                    altText = this._nullAltText;
                    break;
            }

            string cssClass = "tristate";
            if (this.Locked)
            {
                imagePath = this._lockImagePath;
                cssClass += " lockedPerm";

                // altText is set based on Value
            }

            if (!this.SupportsDenyMode)
            {
                cssClass += " noDenyPerm";
            }

            if (this.IsFullControl)
            {
                cssClass += " fullControl";
            }

            if (this.IsView && !this.Locked)
            {
                cssClass += " view";
            }

            if (!string.IsNullOrEmpty(this.PermissionKey) && !this.IsView && !this.IsFullControl)
            {
                cssClass += " " + this.PermissionKey.ToLowerInvariant();
            }

            writer.Write("<img src='{0}' alt='{1}' />", imagePath, altText);

            writer.AddAttribute("class", cssClass);
            base.Render(writer);
        }

        private static void LookupScriptValues(Control ctl, out string grantImagePath, out string denyImagePath, out string nullImagePath, out string lockImagePath, out string grantAltText, out string denyAltText, out string nullAltText)
        {
            grantImagePath = IconController.IconURL("Grant");
            denyImagePath = IconController.IconURL("Deny");
            nullImagePath = IconController.IconURL("Unchecked");
            lockImagePath = IconController.IconURL("Lock");

            grantAltText = Localization.GetString("PermissionTypeGrant");
            denyAltText = Localization.GetString("PermissionTypeDeny");
            nullAltText = Localization.GetString("PermissionTypeNull");
        }
    }
}
