// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    public class Utilities
    {
        public static void ApplySkin(Control telerikControl)
        {
            ApplySkin(telerikControl, string.Empty, string.Empty, string.Empty);
        }

        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName)
        {
            ApplySkin(telerikControl, string.Empty, string.Empty, fallBackEmbeddedSkinName);
        }

        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName, string controlName)
        {
            ApplySkin(telerikControl, string.Empty, controlName, fallBackEmbeddedSkinName);
        }

        // Use selected skin's webcontrol skin if one exists
        // or use _default skin's webcontrol skin if one exists
        // or use embedded skin
        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName, string controlName, string webControlSkinSubFolderName)
        {
            PropertyInfo skinProperty = null;
            PropertyInfo enableEmbeddedSkinsProperty = null;
            bool skinApplied = false;

            try
            {
                skinProperty = telerikControl.GetType().GetProperty("Skin");
                enableEmbeddedSkinsProperty = telerikControl.GetType().GetProperty("EnableEmbeddedSkins");

                if (string.IsNullOrEmpty(controlName))
                {
                    controlName = telerikControl.GetType().BaseType.Name;
                    if (controlName.StartsWith("Rad") || controlName.StartsWith("Dnn"))
                    {
                        controlName = controlName.Substring(3);
                    }
                }

                string skinVirtualFolder = string.Empty;
                if (PortalSettings.Current != null)
                {
                    skinVirtualFolder = PortalSettings.Current.ActiveTab.SkinPath.Replace('\\', '/').Replace("//", "/");
                }
                else
                {
                    skinVirtualFolder = telerikControl.ResolveUrl("~/Portals/_default/skins/_default/Aphelia"); // developer skin Aphelia
                }

                string skinName = string.Empty;
                string webControlSkinName = string.Empty;
                if (skinProperty != null)
                {
                    var v = skinProperty.GetValue(telerikControl, null);
                    if (v != null)
                    {
                        webControlSkinName = v.ToString();
                    }
                }

                if (string.IsNullOrEmpty(webControlSkinName))
                {
                    webControlSkinName = "default";
                }

                if (skinVirtualFolder.EndsWith("/"))
                {
                    skinVirtualFolder = skinVirtualFolder.Substring(0, skinVirtualFolder.Length - 1);
                }

                int lastIndex = skinVirtualFolder.LastIndexOf("/");
                if (lastIndex > -1 && skinVirtualFolder.Length > lastIndex)
                {
                    skinName = skinVirtualFolder.Substring(skinVirtualFolder.LastIndexOf("/") + 1);
                }

                string systemWebControlSkin = string.Empty;
                if (!string.IsNullOrEmpty(skinName) && !string.IsNullOrEmpty(skinVirtualFolder))
                {
                    systemWebControlSkin = HttpContext.Current.Server.MapPath(skinVirtualFolder);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, "WebControlSkin");
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, skinName);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, webControlSkinSubFolderName);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, string.Format("{0}.{1}.css", controlName, webControlSkinName));

                    // Check if the selected skin has the webcontrol skin
                    if (!File.Exists(systemWebControlSkin))
                    {
                        systemWebControlSkin = string.Empty;
                    }

                    // No skin, try default folder
                    if (string.IsNullOrEmpty(systemWebControlSkin))
                    {
                        skinVirtualFolder = telerikControl.ResolveUrl("~/Portals/_default/Skins/_default");
                        skinName = "Default";

                        if (skinVirtualFolder.EndsWith("/"))
                        {
                            skinVirtualFolder = skinVirtualFolder.Substring(0, skinVirtualFolder.Length - 1);
                        }

                        if (!string.IsNullOrEmpty(skinName) && !string.IsNullOrEmpty(skinVirtualFolder))
                        {
                            systemWebControlSkin = HttpContext.Current.Server.MapPath(skinVirtualFolder);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, "WebControlSkin");
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, skinName);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, webControlSkinSubFolderName);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, string.Format("{0}.{1}.css", controlName, webControlSkinName));

                            if (!File.Exists(systemWebControlSkin))
                            {
                                systemWebControlSkin = string.Empty;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(systemWebControlSkin))
                {
                    string filePath = Path.Combine(skinVirtualFolder, "WebControlSkin");
                    filePath = Path.Combine(filePath, skinName);
                    filePath = Path.Combine(filePath, webControlSkinSubFolderName);
                    filePath = Path.Combine(filePath, string.Format("{0}.{1}.css", controlName, webControlSkinName));
                    filePath = filePath.Replace('\\', '/').Replace("//", "/").TrimEnd('/');

                    if (HttpContext.Current != null && HttpContext.Current.Handler is Page)
                    {
                        ClientResourceManager.RegisterStyleSheet(HttpContext.Current.Handler as Page, filePath, FileOrder.Css.ResourceCss);
                    }

                    if ((skinProperty != null) && (enableEmbeddedSkinsProperty != null))
                    {
                        skinApplied = true;
                        skinProperty.SetValue(telerikControl, webControlSkinName, null);
                        enableEmbeddedSkinsProperty.SetValue(telerikControl, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            if (skinProperty != null && enableEmbeddedSkinsProperty != null && !skinApplied)
            {
                if (string.IsNullOrEmpty(fallBackEmbeddedSkinName))
                {
                    fallBackEmbeddedSkinName = "Simple";
                }

                // Set fall back skin Embedded Skin
                skinProperty.SetValue(telerikControl, fallBackEmbeddedSkinName, null);
                enableEmbeddedSkinsProperty.SetValue(telerikControl, true, null);
            }
        }

        public static void CreateThumbnail(FileInfo image, Image img, int maxWidth, int maxHeight)
        {
            if (image.Width > image.Height)
            {
                // Landscape
                if (image.Width > maxWidth)
                {
                    img.Width = maxWidth;
                    img.Height = Convert.ToInt32((image.Height * maxWidth) / (float)image.Width);
                }
                else
                {
                    img.Width = image.Width;
                    img.Height = image.Height;
                }
            }
            else
            {
                // Portrait
                if (image.Height > maxHeight)
                {
                    img.Width = Convert.ToInt32((image.Width * maxHeight) / (float)image.Height);
                    img.Height = maxHeight;
                }
                else
                {
                    img.Width = image.Width;
                    img.Height = image.Height;
                }
            }
        }

        public static string GetClientAlert(Control ctrl, string message)
        {
            return GetClientAlert(ctrl, new MessageWindowParameters(message));
        }

        public static string GetClientAlert(Control ctrl, MessageWindowParameters message)
        {
            return "jQuery(document).ready(function($){$.dnnAlert({ okText: '" + GetLocalizedString("Ok.Text") + "', text: '" + message.Message + "', title: '" + message.Title + "'});});";
        }

        public static string GetLocalizedString(string key)
        {
            string resourceFile = "/App_GlobalResources/WebControls.resx";
            return Localization.GetString(key, resourceFile);
        }

        public static string GetLocalResourceFile(Control ctrl)
        {
            return UIUtilities.GetLocalResourceFile(ctrl);
        }

        public static string GetLocalizedStringFromParent(string key, Control control)
        {
            string returnValue = key;
            string resourceFileName = GetLocalResourceFile(control.Parent);

            if (!string.IsNullOrEmpty(resourceFileName))
            {
                returnValue = Localization.GetString(key, resourceFileName);
            }

            return returnValue;
        }

        public static string GetOnClientClickConfirm(Control ctrl, string message)
        {
            return GetOnClientClickConfirm(ctrl, new MessageWindowParameters(message));
        }

        public static string GetOnClientClickConfirm(Control ctrl, MessageWindowParameters message)
        {
            AddMessageWindow(ctrl);

            // function(text, mozEvent, oWidth, oHeight, callerObj, oTitle)
            return string.Format("return postBackConfirm('{0}', event, '{1}', '{2}', '', '{3}');", message.Message, message.WindowWidth, message.WindowHeight, message.Title);
        }

        public static string GetViewStateAsString(object value, string defaultValue)
        {
            string _Value = defaultValue;
            if (value != null)
            {
                _Value = Convert.ToString(value);
            }

            return _Value;
        }

        public static void RegisterAlertOnPageLoad(Control ctrl, string message)
        {
            RegisterAlertOnPageLoad(ctrl, new MessageWindowParameters(message));
        }

        public static void RegisterAlertOnPageLoad(Control ctrl, MessageWindowParameters message)
        {
            ctrl.Page.ClientScript.RegisterClientScriptBlock(ctrl.GetType(), ctrl.ID + "_AlertOnPageLoad", GetClientAlert(ctrl, message), true);
        }

        private static void AddMessageWindow(Control ctrl)
        {
            ClientResourceManager.RegisterScript(ctrl.Page, ctrl.ResolveUrl("~/js/dnn.postbackconfirm.js"));
        }
    }
}
