// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Dnn.RichTextEditorProvider.Components;
    using Dnn.RichTextEditorProvider.Services;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;

    /// <summary>Renders a rich text editor for a single HTML value and reads the edited value back on postback.</summary>
    public class EditorControl : WebControl, IPostBackDataHandler
    {
        /// <summary>The version of the bundled Jodit editor.</summary>
        public const string JoditVersion = "4.18.10";

        private const string ProviderPath = ProviderSettings.ProviderPath;
        private const string ResourceFile = ProviderPath + "App_LocalResources/Editor.resx";

        /// <summary>Gets or sets the HTML being edited.</summary>
        [DefaultValue("")]
        public string Value
        {
            get => this.ViewState["Value"] as string ?? string.Empty;
            set => this.ViewState["Value"] = value;
        }

        /// <summary>
        /// Gets or sets the toolbar preset (<c>basic</c>, <c>standard</c> or <c>full</c>).
        /// When empty, the preset comes from the provider's <c>toolbar</c> attribute, or else from the current user's roles.
        /// </summary>
        public string ToolbarPreset { get; set; }

        /// <inheritdoc/>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var postedValue = postCollection[postDataKey];
            if (postedValue == null || postedValue.Equals(this.Value, StringComparison.Ordinal))
            {
                return false;
            }

            this.Value = postedValue;
            return true;
        }

        /// <inheritdoc/>
        public void RaisePostDataChangedEvent()
        {
            // Do nothing
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page?.RegisterRequiresPostBack(this);

            if (this.Context.Request.IsAuthenticated)
            {
                // The file browser's requests carry the page's anti-forgery token.
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var providerAttributes = ProviderSettings.GetAttributes();
            var type = this.GetType();

            // ScriptManager (rather than the client resource manager) is used so that the scripts are also delivered
            // when the editor first appears during an UpdatePanel partial postback.
            ScriptManager.RegisterClientScriptInclude(this, type, "DnnJodit", Globals.ResolveUrl($"{ProviderPath}js/jodit/{JoditVersion}/jodit.min.js"));
            ScriptManager.RegisterClientScriptInclude(this, type, "DnnRichTextEditor", Globals.ResolveUrl($"{ProviderPath}js/dnn.richtexteditor.js"));

            var customConfigScript = providerAttributes["customConfigScript"];
            if (!string.IsNullOrEmpty(customConfigScript))
            {
                ScriptManager.RegisterClientScriptInclude(this, type, "DnnRichTextEditorCustomConfig", Globals.ResolveUrl(customConfigScript));
            }

            var clientId = HttpUtility.JavaScriptStringEncode(this.ClientID);
            ScriptManager.RegisterOnSubmitStatement(
                this,
                type,
                $"DnnRichTextEditor_Submit_{this.ClientID}",
                $"if (window.dnn && window.dnn.richTextEditor) {{ window.dnn.richTextEditor.sync('{clientId}'); }}");

            var options = new JavaScriptSerializer().Serialize(this.GetClientOptions(providerAttributes));
            ScriptManager.RegisterStartupScript(
                this,
                type,
                $"DnnRichTextEditor_Create_{this.ClientID}",
                $"if (window.dnn && window.dnn.richTextEditor) {{ window.dnn.richTextEditor.create('{clientId}', {options}); }}",
                true);
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            // The text area remains a usable plain HTML editor if scripts fail to load; the editor hides it once created.
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Cols, "80");
            writer.AddAttribute(HtmlTextWriterAttribute.Rows, "10");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "editor dnn-rte");
            writer.AddAttribute("aria-label", "editor");
            if (!this.Width.IsEmpty)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this.Width.ToString(CultureInfo.InvariantCulture));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
            writer.Write(HttpUtility.HtmlEncode(this.Value));
            writer.RenderEndTag();
        }

        private static bool IsTrue(string value)
        {
            return bool.TryParse(value, out var result) && result;
        }

        private static string ToCssSize(Unit unit)
        {
            return unit.IsEmpty ? null : unit.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetLocalizedString(string key)
        {
            return Localization.GetString(key, ResourceFile);
        }

        private Dictionary<string, object> GetClientOptions(NameValueCollection providerAttributes)
        {
            return new Dictionary<string, object>
            {
                { "stylesheets", new[] { Globals.ResolveUrl($"{ProviderPath}js/jodit/{JoditVersion}/jodit.min.css"), Globals.ResolveUrl($"{ProviderPath}css/dnn.richtexteditor.css"), } },
                { "toolbar", this.GetToolbarPreset(providerAttributes) },
                { "language", CultureInfo.CurrentUICulture.Name },
                { "width", ToCssSize(this.Width) },
                { "height", ToCssSize(this.Height) },
                { "hidePoweredBy", IsTrue(providerAttributes["hidePoweredBy"]) },
                { "portalId", PortalSettings.Current?.PortalId ?? -1 },
                { "fileBrowser", this.GetServiceOptions("FileBrowser") },
                { "pageLinks", this.GetPageLinkOptions() },
            };
        }

        private Dictionary<string, object> GetServiceOptions(string controller)
        {
            // The services need a signed-in user and check permissions on every request.
            if (!this.Context.Request.IsAuthenticated)
            {
                return null;
            }

            return new Dictionary<string, object>
            {
                { "url", $"{ServicesFramework.GetServiceFrameworkRoot()}API/{ServiceRouteMapper.ModuleFolderName}/{controller}/" },
            };
        }

        private Dictionary<string, object> GetPageLinkOptions()
        {
            var options = this.GetServiceOptions("Pages");
            if (options != null)
            {
                options["text"] = new Dictionary<string, string>
                {
                    { "button", GetLocalizedString("PageLinkButton") },
                    { "search", GetLocalizedString("PageLinkSearch") },
                    { "loading", GetLocalizedString("PageLinkLoading") },
                    { "empty", GetLocalizedString("PageLinkEmpty") },
                    { "error", GetLocalizedString("PageLinkError") },
                };
            }

            return options;
        }

        private string GetToolbarPreset(NameValueCollection providerAttributes)
        {
            if (!string.IsNullOrEmpty(this.ToolbarPreset))
            {
                return this.ToolbarPreset;
            }

            if (!string.IsNullOrEmpty(providerAttributes["toolbar"]))
            {
                return providerAttributes["toolbar"];
            }

            // Matches the CKEditor provider's default: the full toolbar for administrators, the basic toolbar for everyone else.
            var portalSettings = PortalSettings.Current;
            var isAdministrator = UserController.Instance.GetCurrentUserInfo().IsSuperUser
                || (portalSettings != null && PortalSecurity.IsInRole(portalSettings.AdministratorRoleName));
            return isAdministrator ? "full" : "basic";
        }
    }
}
