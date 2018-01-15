#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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


#endregion

namespace DotNetNuke.Web.UI
{
    public class Utilities
    {
        #region Private Methods

        private static void AddMessageWindow(Control ctrl)
        {
            ClientResourceManager.RegisterScript(ctrl.Page, ctrl.ResolveUrl("~/js/dnn.postbackconfirm.js"));
        }

        #endregion

        #region Public Methods

        public static void ApplySkin(Control telerikControl)
        {
            ApplySkin(telerikControl, "", "", "");
        }

        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName)
        {
            ApplySkin(telerikControl, "", "", fallBackEmbeddedSkinName);
        }

        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName, string controlName)
        {
            ApplySkin(telerikControl, "", controlName, fallBackEmbeddedSkinName);
        }

        //Use selected skin's webcontrol skin if one exists
        //or use _default skin's webcontrol skin if one exists
        //or use embedded skin
        public static void ApplySkin(Control telerikControl, string fallBackEmbeddedSkinName, string controlName, string webControlSkinSubFolderName)
        {
            PropertyInfo skinProperty = null;
            PropertyInfo enableEmbeddedSkinsProperty = null;
            bool skinApplied = false;

            try
            {
                skinProperty = telerikControl.GetType().GetProperty("Skin");
                enableEmbeddedSkinsProperty = telerikControl.GetType().GetProperty("EnableEmbeddedSkins");

                if ((string.IsNullOrEmpty(controlName)))
                {
                    controlName = telerikControl.GetType().BaseType.Name;
                    if ((controlName.StartsWith("Rad") || controlName.StartsWith("Dnn")))
                    {
                        controlName = controlName.Substring(3);
                    }
                }


                string skinVirtualFolder = "";
                if (PortalSettings.Current != null)
                    skinVirtualFolder = PortalSettings.Current.ActiveTab.SkinPath.Replace('\\', '/').Replace("//", "/");
                else
                    skinVirtualFolder = telerikControl.ResolveUrl("~/Portals/_default/skins/_default/Aphelia"); // developer skin Aphelia

                string skinName = "";
                string webControlSkinName = "";
                if (skinProperty != null)
                {
                    var v = skinProperty.GetValue(telerikControl, null);
                    if (v != null) 
                        webControlSkinName = v.ToString();

                }
                if (string.IsNullOrEmpty(webControlSkinName)) webControlSkinName = "default";

                if ((skinVirtualFolder.EndsWith("/")))
                {
                    skinVirtualFolder = skinVirtualFolder.Substring(0, skinVirtualFolder.Length - 1);
                }
                int lastIndex = skinVirtualFolder.LastIndexOf("/");
                if ((lastIndex > -1 && skinVirtualFolder.Length > lastIndex))
                {
                    skinName = skinVirtualFolder.Substring(skinVirtualFolder.LastIndexOf("/") + 1);
                }

                string systemWebControlSkin = string.Empty;
                if ((!string.IsNullOrEmpty(skinName) && !string.IsNullOrEmpty(skinVirtualFolder)))
                {
					systemWebControlSkin = HttpContext.Current.Server.MapPath(skinVirtualFolder);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, "WebControlSkin");
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, skinName);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, webControlSkinSubFolderName);
                    systemWebControlSkin = Path.Combine(systemWebControlSkin, string.Format("{0}.{1}.css", controlName, webControlSkinName));

                    //Check if the selected skin has the webcontrol skin
                    if ((!File.Exists(systemWebControlSkin)))
                    {
                        systemWebControlSkin = "";
                    }

                    //No skin, try default folder
                    if ((string.IsNullOrEmpty(systemWebControlSkin)))
                    {
                        skinVirtualFolder = telerikControl.ResolveUrl("~/Portals/_default/Skins/_default");
                        skinName = "Default";

                        if ((skinVirtualFolder.EndsWith("/")))
                        {
                            skinVirtualFolder = skinVirtualFolder.Substring(0, skinVirtualFolder.Length - 1);
                        }

                        if ((!string.IsNullOrEmpty(skinName) && !string.IsNullOrEmpty(skinVirtualFolder)))
                        {
                            systemWebControlSkin = HttpContext.Current.Server.MapPath(skinVirtualFolder);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, "WebControlSkin");
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, skinName);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, webControlSkinSubFolderName);
                            systemWebControlSkin = Path.Combine(systemWebControlSkin, string.Format("{0}.{1}.css", controlName, webControlSkinName));

                            if ((!File.Exists(systemWebControlSkin)))
                            {
                                systemWebControlSkin = "";
                            }
                        }
                    }
                }

                if ((!string.IsNullOrEmpty(systemWebControlSkin)))
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

                    if (((skinProperty != null) && (enableEmbeddedSkinsProperty != null)))
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
                if ((string.IsNullOrEmpty(fallBackEmbeddedSkinName)))
                {
                    fallBackEmbeddedSkinName = "Simple";
                }

                //Set fall back skin Embedded Skin
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
                    img.Height = Convert.ToInt32((image.Height*maxWidth)/(float) image.Width);
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
                    img.Width = Convert.ToInt32((image.Width*maxHeight)/(float) image.Height);
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
            //function(text, mozEvent, oWidth, oHeight, callerObj, oTitle) 
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

        #endregion
    }
}