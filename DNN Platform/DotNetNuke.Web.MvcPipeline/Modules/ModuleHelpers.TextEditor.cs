// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ResourceManager;

    public static partial class ModuleHelpers
    {
        public static IHtmlString TextEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            // HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            var id = htmlHelper.IdFor(expression);

            LoadAllSettings(htmlHelper.ViewContext, id.ToString());

            var attrs = new Dictionary<string, object>();
            attrs.Add("id", id);
            attrs.Add("data-ckeditor", true);
            return htmlHelper.TextAreaFor(expression, attrs);
        }

        private static void LoadAllSettings(ViewContext page, string id)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            /*
            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(portalSettings.PortalId);
            NameValueCollection settings = new NameValueCollection();
            int parentModulId = -1;
            string id = "id";

            // Load Default Settings
            var currentEditorSettings = SettingsUtil.GetDefaultSettings(
                portalSettings,
                portalSettings.HomeDirectoryMapPath,
                settings["configFolder"],
                portalRoles);

            // Set Current Mode to Default
            currentEditorSettings.SettingMode = SettingsMode.Default;

            var hostKey = SettingConstants.HostKey;
            var portalKey = SettingConstants.PortalKey(portalSettings.PortalId);
            var pageKey = $"DNNCKT#{portalSettings.ActiveTab.TabID}#";
            var moduleKey = $"DNNCKMI#{parentModulId}#INS#{id}#";

            // Load Host Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, hostKey))
            {
                var hostPortalRoles = RoleController.Instance.GetRoles(Host.HostPortalID);
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings,
                    currentEditorSettings,
                    settingsDictionary,
                    hostKey,
                    hostPortalRoles);

                // Set Current Mode to Host
                currentEditorSettings.SettingMode = SettingsMode.Host;

                // reset the roles to the correct portal
                if (portalSettings.PortalId != Host.HostPortalID)
                {
                    foreach (var toolbarRole in currentEditorSettings.ToolBarRoles)
                    {
                        var roleName = hostPortalRoles.FirstOrDefault(role => role.RoleID == toolbarRole.RoleId)?.RoleName ?? string.Empty;
                        var roleId = portalRoles.FirstOrDefault(role => role.RoleName.Equals(roleName))?.RoleID ?? Null.NullInteger;
                        toolbarRole.RoleId = roleId;
                    }

                    foreach (var uploadRoles in currentEditorSettings.UploadSizeRoles)
                    {
                        var roleName = hostPortalRoles.FirstOrDefault(role => role.RoleID == uploadRoles.RoleId)?.RoleName ?? string.Empty;
                        var roleId = portalRoles.FirstOrDefault(role => role.RoleName.Equals(roleName))?.RoleID ?? Null.NullInteger;
                        uploadRoles.RoleId = roleId;
                    }
                }
            }

            // Load Portal Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, portalKey))
            {
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings,
                    currentEditorSettings,
                    settingsDictionary,
                    portalKey,
                    portalRoles);

                // Set Current Mode to Portal
                currentEditorSettings.SettingMode = SettingsMode.Portal;
            }

            // Load Page Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey))
            {
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings, currentEditorSettings, settingsDictionary, pageKey, portalRoles);

                // Set Current Mode to Page
                currentEditorSettings.SettingMode = SettingsMode.Page;
            }

            // Load Module Settings ?!
            if (!SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, parentModulId))
            {
                return;
            }

            currentEditorSettings = SettingsUtil.LoadModuleSettings(
                portalSettings, currentEditorSettings, moduleKey, parentModulId, portalRoles);

            // Set Current Mode to Module Instance
            currentEditorSettings.SettingMode = SettingsMode.ModuleInstance;
            */

            if (!page.HttpContext.Request.IsAjaxRequest())

            //if (page.IsChildAction)
            {
                var controller = GetClientResourcesController();
                controller.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorToolBars.css");
                controller.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorOverride.css");
                controller.RegisterStylesheet("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/editor.css");

                /*
                const string CsName = "CKEdScript";
                const string CsFindName = "CKFindScript";
                */

                JavaScript.RequestRegistration(CommonJs.jQuery);

                controller.RegisterScript("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/ckeditor.js");
                controller.RegisterScript("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/editorOverride.js");
            }

            /*
            // Load Custom JS File
            if (!string.IsNullOrEmpty(currentEditorSettings.CustomJsFile))
            {
                MvcClientResourceManager.RegisterScript(page, FormatUrl(portalSettings, currentEditorSettings.CustomJsFile));
            }

            // GenerateEditorLoadScript
            var clientID = "cke1";
            var editorVar = string.Format(
                "editor{0}",
                clientID.Substring(clientID.LastIndexOf("_", StringComparison.Ordinal) + 1).Replace(
                    "-", string.Empty));

            var editorFixedId = clientID.Replace("-", string.Empty).Replace(".", string.Empty);

            var postBackScript = string.Format(
                @" if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();  if (typeof Page_IsValid !== 'undefined' && !Page_IsValid) return false; CKEDITOR.instances.{0}.destroy(); }}",
                editorFixedId);
            */
            /*
            this.RegisterOnSubmitStatement(
                this.GetType(), string.Format("CKEditor_OnAjaxSubmit_{0}", editorFixedId), postBackScript);
            */
            var editorScript = new StringBuilder();
            /*
            editorScript.AppendFormat(
                "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(LoadCKEditorInstance_{0});", editorFixedId);

            editorScript.AppendFormat("function LoadCKEditorInstance_{0}(sender,args) {{", editorFixedId);

            editorScript.AppendFormat(
                @"if (jQuery(""[id*='UpdatePanel']"").length == 0 && CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();}}",
                editorFixedId);

            editorScript.AppendFormat(
                "if (document.getElementById('{0}') == null){{return;}}",
                editorFixedId);

            // Render EditorConfig
            var editorConfigScript = new StringBuilder();
            editorConfigScript.AppendFormat("var editorConfig{0} = {{", editorVar);

            var keysCount = settings.Keys.Count;
            var currentCount = 0;

            // Write options
            foreach (string key in settings.Keys)
            {
                var value = settings[key];

                currentCount++;

                // Is boolean state or string
                if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals("false", StringComparison.InvariantCultureIgnoreCase) || value.StartsWith("[")
                    || value.StartsWith("{") || Utility.IsNumeric(value))
                {
                    if (value.Equals("True"))
                    {
                        value = "true";
                    }
                    else if (value.Equals("False"))
                    {
                        value = "false";
                    }

                    editorConfigScript.AppendFormat("{0}:{1}", key, value);

                    editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
                }
                else
                {
                    if (key == "browser")
                    {
                        continue;
                    }

                    editorConfigScript.AppendFormat("{0}:\'{1}\'", key, value);

                    editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
                }
            }

            editorScript.AppendFormat(
                "if (CKEDITOR.instances.{0}){{return;}}",
                editorFixedId);

            // Check if we can use jQuery or $, and if both fail use ckeditor without the adapter
            editorScript.Append("if (jQuery().ckeditor) {");

            editorScript.AppendFormat("var {0} = jQuery('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else if ($.ckeditor) {");

            editorScript.AppendFormat("var {0} = $('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else {");

            editorScript.AppendFormat("var {0} = CKEDITOR.replace( '{1}', editorConfig{0});", editorVar, editorFixedId);

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
            editorScript.Append("  CKEDITOR.config.portalId = " + portalSettings.PortalId);
            editorScript.Append("};");

            // End of LoadScript
            editorScript.Append("}");

            MvcClientAPI.RegisterScript(string.Format(@"{0}_CKE_Config", editorFixedId), editorConfigScript.ToString());
            MvcClientAPI.RegisterStartupScript(string.Format(@"{0}_CKE_Startup", editorFixedId), editorScript.ToString());
            */
            /*
            editorScript.Append(@"
                if(CKEDITOR && CKEDITOR.config){
                    CKEDITOR.config.portalId = " + portalSettings.PortalId + @";
                    CKEDITOR.config.height = '400px';
                    CKEDITOR.config.toolbar = [
                        { name: 'document', items: [ 'Source', '-', 'Save', 'NewPage', 'Preview', 'Print', '-', 'Templates' ] },
                        { name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo' ] },
                        { name: 'editing', items: [ 'Find', 'Replace', '-', 'SelectAll', '-', 'Scayt' ] },
                        { name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat' ] },
                        '/',
                        { name: 'paragraph', items: [ 'NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl' ] },
                        { name: 'links', items: [ 'Link', 'Unlink', 'Anchor' ] },
                        { name: 'insert', items: [ 'Image', 'Table', 'HorizontalRule', 'SpecialChar' ] },
                        '/',
                        { name: 'styles', items: [ 'Styles', 'Format', 'Font', 'FontSize' ] },
                        { name: 'colors', items: [ 'TextColor', 'BGColor' ] },
                        { name: 'tools', items: [ 'Maximize', 'ShowBlocks' ] }
                    ];
                    CKEDITOR.config.removePlugins = 'elementspath,resize';
                    CKEDITOR.config.extraPlugins = 'dnnpages';
                    CKEDITOR.config.allowedContent = true;
                }
                jQuery('[data-ckeditor]').each(function() {
                    CKEDITOR.replace(this.id);
                });
            ");

            MvcClientAPI.RegisterStartupScript("CKEditorConfig", editorScript.ToString());
            */
        }

        /*
        private static string FormatUrl(PortalSettings portalSettings, string inputUrl)
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

                formattedUrl = portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }
            else
            {
                formattedUrl = portalSettings.HomeDirectory + inputUrl;
            }

            return formattedUrl;
        }
        */
    }
}
