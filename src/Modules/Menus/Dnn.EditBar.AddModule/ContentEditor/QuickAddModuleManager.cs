﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace Dnn.EditBar.AddModule.ContentEditor
{
    public class QuickAddModuleManager : UserControlBase
    {
        #region Fields

        private const string ControlFolder = "~/admin/Dnn.EditBar/Resources/QuickAddModuleManager";
        private const string SharedResourceFile = "~/admin/Dnn.EditBar/App_LocalResources/AddModule.resx";

        #endregion

        internal static QuickAddModuleManager GetCurrent(Page page)
        {
            if (page.Items.Contains("QuickAddModuleManager"))
            {
                return page.Items["QuickAddModuleManager"] as QuickAddModuleManager;
            }

            return null;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (GetCurrent(Page) != null)
            {
                throw new Exception("Instance has already initialized");
            }

            var isSpecialPageMode = Request.QueryString["dnnprintmode"] == "true" || Request.QueryString["popUp"] == "true";
            if (isSpecialPageMode || PortalSettings.UserMode != PortalSettings.Mode.Edit
                || !TabPermissionController.CanAddContentToPage())
            {
                Parent.Controls.Remove(this);
                return;
            }

            Page.Items.Add("QuickAddModuleManager", this);

            EnsureChildControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (PortalSettings.UserMode != PortalSettings.Mode.Edit)
            {
                return;
            }

            ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "Js/QuickAddModule.js"));
            ClientResourceManager.RegisterStyleSheet(Page, Path.Combine(ControlFolder, "Styles/QuickAddModule.css"));

            if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
            {
                ClientResourceManager.RegisterScript(Page, Path.Combine(ControlFolder, "Js/ImagesUploader.js"));
                ClientResourceManager.RegisterStyleSheet(Page, Path.Combine(ControlFolder, "Styles/ImagesUploader.css"));
            }

            jQuery.RegisterFileUpload(Page);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            RegisterInitScripts();
        }

        private static void FindControlRecursive(Control rootControl, string controlId, ICollection<Control> foundControls)
        {

            if (rootControl.ID == controlId)
            { 
                foundControls.Add(rootControl);
            }

            foreach(Control subControl in rootControl.Controls)
            {
                FindControlRecursive(subControl, controlId, foundControls);
            }
        }

        private string GetPanesClientIds(IEnumerable<IEnumerable<string>> panelCliendIdCollection)
        {
            return string.Join(";", panelCliendIdCollection.Select(x => String.Join(",", x)));
        }

        private IEnumerable<IEnumerable<string>> GetPaneClientIdCollection()
        {
            var panelClientIds = new List<List<string>>(PortalSettings.ActiveTab.Panes.Count);

            try
            {
                var skinControl = Page.FindControl("SkinPlaceHolder").Controls[0];

                foreach (var pane in PortalSettings.ActiveTab.Panes.Cast<string>())
                {
                    var foundControls = new List<Control>();
                    FindControlRecursive(skinControl, pane, foundControls);
                    panelClientIds.Add((from control in foundControls select control.ClientID).ToList());
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return panelClientIds;
        }

        private void RegisterInitScripts()
        {
            var panes = string.Join(",", PortalSettings.ActiveTab.Panes.Cast<string>());
            var panesClientIds = GetPanesClientIds(GetPaneClientIdCollection());
            var page = PortalSettings.ActiveTab.TabID;
            var textModule = DesktopModuleController.GetDesktopModuleByModuleName("DNN_HTML", PortalSettings.PortalId).DesktopModuleID;
            var imgModule = DesktopModuleController.GetDesktopModuleByModuleName("DNN_HTML", PortalSettings.PortalId).DesktopModuleID;
            var addModuleTitle = Localization.GetSafeJSString("AddModuleTitle", SharedResourceFile);
            var addTextModuleTitle = Localization.GetSafeJSString("AddTextModuleTitle", SharedResourceFile);
            var addImageModuleTitle = Localization.GetSafeJSString("AddImageModuleTitle", SharedResourceFile);
            const string scriptFormat = @"dnn.QuickAddModuleManager.init({{
                                    type: 'QuickAddModuleManager', 
                                    panes: '{0}',
                                    panesClientIds: '{1}', 
                                    page: {2}, 
                                    textModule: {3}, 
                                    imgModule: {4}, 
                                    addModuleTitle: '{5}',
                                    addTextModuleTitle: '{6}',
                                    addImageModuleTitle: '{7}'}});";
            var script = string.Format(scriptFormat, panes, panesClientIds, page, textModule, imgModule, addModuleTitle, addTextModuleTitle, addImageModuleTitle);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager.RegisterStartupScript(Page, GetType(), "QuickAddModuleManager", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "QuickAddModuleManager", script, true);
            }

            //Get all localized strings used by ImagesUploader.js 
            var configMaxFileSize = (int) Config.GetMaxUploadSize();
            var quickResources = new
            {
                invalidFileMessage = Localization.GetString("QInvalidFileMessage", SharedResourceFile),
                imageExtensionsAllowed = Localization.GetString("QImageExtensions", SharedResourceFile),
                imageProgressTitle = Localization.GetString("QImageProgressTitle", SharedResourceFile),
                imageProgress = Localization.GetString("QImageProgress", SharedResourceFile),
                fileUploadFailed = Localization.GetString("QUploadFailed", SharedResourceFile),
                maxFileSize = configMaxFileSize,
                fileTooLarge = string.Format(Localization.GetString("QFileIsTooLarge", SharedResourceFile) + " Mb",
                    (configMaxFileSize / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)),
                fileAlertConfirmTitle = Localization.GetString("QUploadTitle", SharedResourceFile),
                confirmQuestion = Localization.GetString("QContinueQuestion", SharedResourceFile)
            }.ToJson();

            var resx = @"dnn.ImagesUploaderResources = " + quickResources + ";";

            
            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager.RegisterStartupScript(Page, GetType(), "ImagesUploaderResources", resx, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "ImagesUploaderResources", resx, true);
            }
        }
    }
}
