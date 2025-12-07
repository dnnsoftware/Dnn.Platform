// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web;

    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml.Serialization;

    using DNNConnect.CKEditorProvider.Constants;
    using DNNConnect.CKEditorProvider.Extensions;
    using DNNConnect.CKEditorProvider.Objects;
    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The CKEditor control.</summary>
    public class EditorControl : WebControl, IPostBackDataHandler
    {
        private const string ProviderType = "htmlEditor";
        private readonly INavigationManager navigationManager;
        private readonly PortalSettings portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
        private bool isMerged; // Check if the Settings Collection is Merged with all Settings.
        private NameValueCollection settings;
        private EditorProviderSettings currentEditorSettings = new EditorProviderSettings();
        private string toolBarNameOverride;
        private PortalModuleBase portalModule;
        private int parentModulId;

        /// <summary>Initializes a new instance of the <see cref="EditorControl"/> class.</summary>
        public EditorControl()
        {
            this.navigationManager = this.Context.GetScope().ServiceProvider.GetRequiredService<INavigationManager>();
            this.settings = SettingsLoader.LoadConfigSettings(ProviderType);
            this.Init += this.CKEditorInit;
        }

        /// <summary>Gets a value indicating whether CKEditor is rendered.</summary>
        public bool IsRendered { get; private set; }

        /// <summary>Gets the editor settings.</summary>
        public NameValueCollection Settings
        {
            get
            {
                if (this.isMerged)
                {
                    return this.settings;
                }

                // Convert AttributeCollection to NameValueCollection
                var attributesCollection = new NameValueCollection();
                foreach (string key in this.Attributes.Keys)
                {
                    attributesCollection[key] = this.Attributes[key];
                }

                SettingsLoader.PopulateSettings(
                    this.settings,
                    this.currentEditorSettings,
                    this.portalSettings,
                    this.portalModule?.ModuleConfiguration,
                    attributesCollection,
                    this.Width,
                    this.Height,
                    this.ID,
                    this.parentModulId,
                    this.toolBarNameOverride);

                this.isMerged = true;

                return this.settings;
            }
        }

        /// <summary> Gets or sets The ToolBarName defined in config to override all other Toolbars.</summary>
        public string ToolBarName
        {
            get
            {
                return this.toolBarNameOverride;
            }

            set
            {
                this.toolBarNameOverride = value;
            }
        }

        /// <summary>Gets or sets Value.</summary>
        [DefaultValue("")]
        public string Value
        {
            get
            {
                object o = this.ViewState["Value"];

                return o == null ? string.Empty : Convert.ToString(o);
            }

            set
            {
                this.ViewState["Value"] = value;
            }
        }

        private static string GetResxFileName
        {
            get
            {
                return
                    Globals.ResolveUrl(
                        string.Format("~/Providers/HtmlEditorProviders/DNNConnect.CKE/{0}/Options.aspx.resx", Localization.LocalResourceDirectory));
            }
        }

        /// <summary>Finds the module instance.</summary>
        /// <param name="editorControl">The editor control.</param>
        /// <returns>The Instances found.</returns>
        public static Control FindModuleInstance(Control editorControl)
        {
            Control ctl = editorControl.Parent;
            Control selectedCtl = null;
            Control possibleCtl = null;

            while (ctl != null)
            {
                var portalModuleBase = ctl as PortalModuleBase;

                if (portalModuleBase != null)
                {
                    if (portalModuleBase.TabModuleId == Null.NullInteger)
                    {
                        possibleCtl = ctl;
                    }
                    else
                    {
                        selectedCtl = ctl;
                        break;
                    }
                }

                ctl = ctl.Parent;
            }

            if (selectedCtl == null & possibleCtl != null)
            {
                selectedCtl = possibleCtl;
            }

            return selectedCtl;
        }

        /// <summary>Checks if the text area was rendered.</summary>
        /// <param name="control">The control to ckeck.</param>
        /// <returns>Returns a value indicating whether the text area has rendered.</returns>
        public bool HasRenderedTextArea(Control control)
        {
            if (control is EditorControl && ((EditorControl)control).IsRendered)
            {
                return true;
            }

            return control.Controls.Cast<Control>().Any(this.HasRenderedTextArea);
        }

        /// <summary>Loads the post data.</summary>
        /// <param name="postDataKey">The post data key.</param>
        /// <param name="postCollection">The post collection.</param>
        /// <returns>A value indicating whether the PostData has loaded.</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            try
            {
                string currentValue = this.Value;
                string postedValue = postCollection[postDataKey];

                if (currentValue == null | !postedValue.Equals(currentValue))
                {
                    if (this.currentEditorSettings.InjectSyntaxJs)
                    {
                        if (postedValue.Contains("<pre class=\"brush:") && !postedValue.Contains("shCore.js"))
                        {
                            // Add Syntax Highlighter Plugin
                            postedValue =
                                string.Format(
                                    "<!-- Injected Syntax Highlight Code --><script type=\"text/javascript\" src=\"{0}plugins/syntaxhighlight/scripts/shCore.js\"></script><link type=\"text/css\" rel=\"stylesheet\" href=\"{0}plugins/syntaxhighlight/styles/shCore.css\"/><script type=\"text/javascript\">SyntaxHighlighter.all();</script>{1}",
                                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditor/"),
                                    postedValue);
                        }

                        if (postedValue.Contains("<span class=\"math-tex\">") && !postedValue.Contains("MathJax.js"))
                        {
                            // Add MathJax Plugin
                            postedValue =
                                string.Format(
                                    "<!-- Injected MathJax Code --><script type=\"text/javascript\" src=\"//cdn.mathjax.org/mathjax/2.2-latest/MathJax.js?config=TeX-AMS_HTML\"></script>{0}",
                                    postedValue);
                        }
                    }

                    this.Value = postedValue;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>Taises post data changed event.</summary>
        public void RaisePostDataChangedEvent()
        {
            // Do nothing
        }

        /// <summary>
        /// Update the Editor after the Post back
        /// And Create Main Script to Render the Editor.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        /// <summary>Renders the control.</summary>
        /// <param name="outWriter">The Writer to render to.</param>
        protected override void Render(HtmlTextWriter outWriter)
        {
            outWriter.Write("<div>");
            outWriter.Write("<noscript>");
            outWriter.Write("<p>");
            outWriter.Write(Localization.GetString("NoJava.Text", GetResxFileName));
            outWriter.Write("</p>");
            outWriter.Write("</noscript>");
            outWriter.Write("</div>");

            outWriter.Write(outWriter.NewLine);

            var styleWidth = !string.IsNullOrEmpty(this.currentEditorSettings.Config.Width)
                                 ? string.Format(" style=\"width:{0};\"", this.currentEditorSettings.Config.Width)
                                 : string.Empty;

            outWriter.Write("<div{0}>", styleWidth);

            // Write text area
            outWriter.AddAttribute("id", this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty));
            outWriter.AddAttribute("name", this.UniqueID);

            outWriter.AddAttribute("cols", "80");
            outWriter.AddAttribute("rows", "10");

            outWriter.AddAttribute("class", "editor");
            outWriter.AddAttribute("aria-label", "editor");

            outWriter.AddAttribute("style", "visibility: hidden; display: none;");

            outWriter.RenderBeginTag("textarea");

            if (string.IsNullOrEmpty(this.Value))
            {
                if (!string.IsNullOrEmpty(this.currentEditorSettings.BlankText))
                {
                    outWriter.Write(this.Context.Server.HtmlEncode(this.currentEditorSettings.BlankText));
                }
            }
            else
            {
                outWriter.Write(this.Context.Server.HtmlEncode(this.Value));
            }

            outWriter.RenderEndTag();

            outWriter.Write("</div>");

            this.IsRendered = true;

            if (!this.HasRenderedTextArea(this.Page))
            {
                return;
            }

            outWriter.Write("<p style=\"text-align:center;\">");

            if (PortalSecurity.IsInRoles(this.portalSettings.AdministratorRoleName))
            {
                var editorUrl = this.navigationManager.NavigateURL(
                    "CKEditorOptions",
                    "ModuleId=" + this.parentModulId,
                    "minc=" + this.ID,
                    "PortalID=" + this.portalSettings.PortalId,
                    "langCode=" + CultureInfo.CurrentCulture.Name,
                    "popUp=true");

                outWriter.Write(
                    "<a href=\"javascript:void(0)\" onclick='window.open({0},\"Options\", \"width=850,height=750,resizable=yes\")' class=\"CommandButton\" id=\"{1}\">{2}</a>",
                    HttpUtility.HtmlAttributeEncode(HttpUtility.JavaScriptStringEncode(editorUrl, true)),
                    string.Format("{0}_ckoptions", this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty)),
                    Localization.GetString("Options.Text", GetResxFileName));
            }

            outWriter.Write("</p>");
        }

        private string GetContainerSourcePath()
        {
            var containerSource = this.portalSettings.ActiveTab.ContainerSrc ?? this.portalSettings.DefaultPortalContainer;
            containerSource = this.ResolveSourcePath(containerSource);
            return containerSource;
        }

        private string GetSkinSourcePath()
        {
            var skinSource = this.portalSettings.ActiveTab.SkinSrc ?? this.portalSettings.DefaultPortalSkin;
            skinSource = this.ResolveSourcePath(skinSource);
            return skinSource;
        }

        private string ResolveSourcePath(string source)
        {
            source = "~" + source;
            return source;
        }

        private void CKEditorInit(object sender, EventArgs e)
        {
            this.Page?.RegisterRequiresPostBack(this); // Ensures that postback is handled

            this.portalModule = (PortalModuleBase)FindModuleInstance(this);

            if (this.portalModule == null || this.portalModule.ModuleId == -1)
            {
                // Get Parent ModuleID From this ClientID
                string sClientId = this.ClientID.Substring(this.ClientID.IndexOf("ctr") + 3);

                sClientId = sClientId.Remove(this.ClientID.IndexOf("_"));

                if (!int.TryParse(sClientId, out this.parentModulId))
                {
                    // The is no real module, then use the "User Accounts" module (Profile editor)
                    ModuleController db = new ModuleController();
                    ModuleInfo objm = db.GetModuleByDefinition(this.portalSettings.PortalId, "User Accounts");

                    this.parentModulId = objm.TabModuleID;
                }
            }
            else
            {
                this.parentModulId = this.portalModule.ModuleId;
            }

            this.currentEditorSettings = SettingsLoader.LoadSettings(
                this.portalSettings,
                this.parentModulId,
                this.ID,
                this.settings["configFolder"]);
            this.RegisterCKEditorLibrary();
            this.GenerateEditorLoadScript();
        }

        /// <summary>Format the URL from FileID to File Path URL.</summary>
        /// <param name="inputUrl">The Input URL.</param>
        /// <returns>The formatted URL.</returns>
        private string FormatUrl(string inputUrl)
        {
            var formattedUrl = string.Empty;

            if (string.IsNullOrEmpty(inputUrl))
            {
                return formattedUrl;
            }

            if (inputUrl.StartsWith("http://") || inputUrl.StartsWith("https://") || inputUrl.StartsWith("//"))
            {
                formattedUrl = inputUrl;
            }
            else if (inputUrl.StartsWith("FileID="))
            {
                var fileId = int.Parse(inputUrl.Substring(7));

                var objFileInfo = FileManager.Instance.GetFile(fileId);

                formattedUrl = this.portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }
            else
            {
                formattedUrl = this.portalSettings.HomeDirectory + inputUrl;
            }

            return formattedUrl;
        }

        private void RegisterStartupScript(string key, string script, bool addScriptTags)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, addScriptTags);
        }

        private void RegisterScript(string key, string script, bool addScriptTags)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), key, script, addScriptTags);
        }

        private void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            ScriptManager.RegisterOnSubmitStatement(this, type, key, script);
        }

        private bool CanUseFullToolbarAsDefault()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return false;
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            return currentUser.IsSuperUser || PortalSecurity.IsInRole(this.portalSettings.AdministratorRoleName);
        }

        private void RegisterCKEditorLibrary()
        {
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorToolBars.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorOverride.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/editor.css"));

            ClientScriptManager cs = this.Page.ClientScript;

            Type csType = this.GetType();

            const string CsName = "CKEdScript";
            const string CsFindName = "CKFindScript";

            JavaScript.RequestRegistration(CommonJs.jQuery);

            if (File.Exists(this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/ckeditor.js"))
                && !cs.IsClientScriptIncludeRegistered(csType, CsName))
            {
                cs.RegisterClientScriptInclude(
                    csType, CsName, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/ckeditor.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js")) &&
                !cs.IsClientScriptIncludeRegistered(csType, CsFindName) && this.currentEditorSettings.BrowserMode.Equals(BrowserType.CKFinder))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    CsFindName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js"));
            }

            ClientResourceManager.RegisterScript(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/editorOverride.js"));

            // Load Custom JS File
            if (!string.IsNullOrEmpty(this.currentEditorSettings.CustomJsFile)
                && !cs.IsClientScriptIncludeRegistered(csType, "CKCustomJSFile"))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    "CKCustomJSFile",
                    this.FormatUrl(this.currentEditorSettings.CustomJsFile));
            }
        }

        private void GenerateEditorLoadScript()
        {
            var editorVar = $"editor{this.ClientID.Substring(this.ClientID.LastIndexOf("_", StringComparison.Ordinal) + 1).Replace("-", string.Empty)}";

            var editorFixedId = this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty);

            var postBackScript = string.Format(
                @" if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();  if (typeof Page_IsValid !== 'undefined' && !Page_IsValid) return false; CKEDITOR.instances.{0}.destroy(); }}",
                editorFixedId);

            this.RegisterOnSubmitStatement(this.GetType(), $"CKEditor_OnAjaxSubmit_{editorFixedId}", postBackScript);

            var editorScript = new StringBuilder();

            editorScript.AppendFormat(
                "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(LoadCKEditorInstance_{0});", editorFixedId);

            editorScript.AppendFormat("function LoadCKEditorInstance_{0}(sender,args) {{", editorFixedId);

            editorScript.AppendFormat(
                @"if (jQuery(""[id*='UpdatePanel']"").length == 0 && CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();}}",
                editorFixedId);

            editorScript.AppendFormat(
                "if (document.getElementById('{0}') == null){{return;}}",
                HttpUtility.JavaScriptStringEncode(editorFixedId));

            // Render EditorConfig
            string editorConfigScript = SettingsLoader.GetEditorConfigScript(this.Settings, editorVar);

            editorScript.AppendFormat(
                "if (CKEDITOR.instances.{0}){{return;}}",
                editorFixedId);

            // Check if we can use jQuery or $, and if both fail use ckeditor without the adapter
            editorScript.Append("if (jQuery().ckeditor) {");

            editorScript.AppendFormat("var {0} = jQuery('#{1}').ckeditor(editorConfig{0});", editorVar, HttpUtility.JavaScriptStringEncode(editorFixedId));

            editorScript.Append("} else if ($.ckeditor) {");

            editorScript.AppendFormat("var {0} = $('#{1}').ckeditor(editorConfig{0});", editorVar, HttpUtility.JavaScriptStringEncode(editorFixedId));

            editorScript.Append("} else {");

            editorScript.AppendFormat("var {0} = CKEDITOR.replace( '{1}', editorConfig{0});", editorVar, HttpUtility.JavaScriptStringEncode(editorFixedId));

            editorScript.Append("}");

            // firefox maximize fix
            editorScript.Append("CKEDITOR.on('instanceReady', function (ev) {");
            editorScript.Append("ev.editor.on('maximize', function () {");
            editorScript.Append("if (ev.editor.commands.maximize.state == 1) {");
            editorScript.Append("var mainDocument = CKEDITOR.document;");
            editorScript.Append("CKEDITOR.env.gecko && mainDocument.getDocumentElement().setStyle( 'position', 'fixed' );");
            editorScript.Append("}");
            editorScript.Append("});");
            editorScript.Append("});");

            editorScript.Append("if(CKEDITOR && CKEDITOR.config){");
            editorScript.AppendFormat("  CKEDITOR.config.portalId = {0}", this.portalSettings.PortalId);
            editorScript.Append("};");

            // End of LoadScript
            editorScript.Append("}");

            this.RegisterScript($@"{editorFixedId}_CKE_Config", editorConfigScript, true);
            this.RegisterStartupScript($@"{editorFixedId}_CKE_Startup", editorScript.ToString(), true);
        }
    }
}
