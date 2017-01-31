#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Host;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Framework
{
    public class AJAX
    {
        #region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddScriptManager is used internally by the framework to add a ScriptManager control to the page
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static void AddScriptManager(Page page)
        {
	        AddScriptManager(page, true);
        }
		/// <summary>
		/// AddScriptManager is used internally by the framework to add a ScriptManager control to the page.
		/// </summary>
		/// <param name="page">the page instance.</param>
		/// <param name="checkCdn">Whether check cdn settings from host settings.</param>
        public static void AddScriptManager(Page page, bool checkCdn)
        {
			if (GetScriptManager(page) == null)
            {
                if (page.Form != null)
                {
                    try
                    {
                        using (var scriptManager = new ScriptManager //RadScriptManager
                                {
                                    ID = "ScriptManager",
                                    EnableScriptGlobalization = true,
                                    SupportsPartialRendering = true,
                                    //EnableHandlerDetection = false //DNN-9145 TODO
                                })
                        {
                            if (checkCdn)
                            {
                                scriptManager.EnableCdn = Host.EnableMsAjaxCdn;
                                scriptManager.EnableCdnFallback = Host.EnableMsAjaxCdn;
                                //DNN-9145 TODO
                                //scriptManager.CdnSettings.TelerikCdn = Host.EnableTelerikCdn ? TelerikCdnMode.Enabled : TelerikCdnMode.Disabled;
                                //if (scriptManager.CdnSettings.TelerikCdn != TelerikCdnMode.Disabled && !string.IsNullOrEmpty(Host.TelerikCdnBasicUrl))
                                //{
                                //	scriptManager.CdnSettings.BaseUrl = Host.TelerikCdnBasicUrl;
                                //}
                                //if (scriptManager.CdnSettings.TelerikCdn != TelerikCdnMode.Disabled && !string.IsNullOrEmpty(Host.TelerikCdnSecureUrl))
                                //{
                                //	scriptManager.CdnSettings.BaseSecureUrl = Host.TelerikCdnSecureUrl;
                                //}
                            }
                            page.Form.Controls.AddAt(0, scriptManager);
                        }
                        if (HttpContext.Current.Items["System.Web.UI.ScriptManager"] == null)
                        {
                            HttpContext.Current.Items.Add("System.Web.UI.ScriptManager", true);
                        }
                    }
                    catch
                    {
                        //suppress error adding script manager to support edge-case of module developers custom aspx pages that inherit from basepage and use code blocks
                    }
                }
                if (page.Form != null)
                {
                    try
                    {
                        //DNN-9145 TODO
                        //using (var stylesheetManager = new RadStyleSheetManager { ID = "StylesheetManager", EnableHandlerDetection = false })
                        //{
                        //	if (checkCdn)
                        //	{
                        //		stylesheetManager.CdnSettings.TelerikCdn = Host.EnableTelerikCdn ? TelerikCdnMode.Enabled : TelerikCdnMode.Disabled;
                        //		if (stylesheetManager.CdnSettings.TelerikCdn != TelerikCdnMode.Disabled && !string.IsNullOrEmpty(Host.TelerikCdnBasicUrl))
                        //		{
                        //			stylesheetManager.CdnSettings.BaseUrl = Host.TelerikCdnBasicUrl;
                        //		}
                        //		if (stylesheetManager.CdnSettings.TelerikCdn != TelerikCdnMode.Disabled && !string.IsNullOrEmpty(Host.TelerikCdnSecureUrl))
                        //		{
                        //			stylesheetManager.CdnSettings.BaseSecureUrl = Host.TelerikCdnSecureUrl;
                        //		}
                        //	}
                        //	page.Form.Controls.AddAt(0, stylesheetManager);
                        //}
                    }
                    catch
                    {
                        //suppress error adding script manager to support edge-case of module developers custom aspx pages that inherit from basepage and use code blocks
                    }
                }
            }
        }

	/// <summary>Gets the current ScriptManager on the page</summary>
	/// <param name="objPage">the page instance.</param>
	/// <returns>The ScriptManager instance, or <c>null</c></returns>
        public static ScriptManager GetScriptManager(Page objPage)
        {
            return objPage.FindControl("ScriptManager") as ScriptManager;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   IsEnabled can be used to determine if AJAX has been enabled already as we
        ///   only need one Script Manager per page.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static bool IsEnabled()
        {
            if (HttpContext.Current.Items["System.Web.UI.ScriptManager"] == null)
            {
                return false;
            }
            else
            {
                return (bool) HttpContext.Current.Items["System.Web.UI.ScriptManager"];
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   IsInstalled can be used to determine if AJAX is installed on the server
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static bool IsInstalled()
        {
            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Allows a control to be excluded from UpdatePanel async callback
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static void RegisterPostBackControl(Control objControl)
        {
            ScriptManager objScriptManager = GetScriptManager(objControl.Page);
            if (objScriptManager != null)
            {
                objScriptManager.RegisterPostBackControl(objControl);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   RegisterScriptManager must be used by developers to instruct the framework that AJAX is required on the page
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static void RegisterScriptManager()
        {
            if (!IsEnabled())
            {
                HttpContext.Current.Items.Add("System.Web.UI.ScriptManager", true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   RemoveScriptManager will remove the ScriptManager control during Page Render if the RegisterScriptManager has not been called
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static void RemoveScriptManager(Page objPage)
        {
            if (!IsEnabled())
            {
                Control objControl = objPage.FindControl("ScriptManager");
                if ((objControl != null))
                {
                    objPage.Form.Controls.Remove(objControl);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Wraps a control in an update panel
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static Control WrapUpdatePanelControl(Control objControl, bool blnIncludeProgress)
        {
            var updatePanel = new UpdatePanel();
            updatePanel.ID = objControl.ID + "_UP";
            updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

            Control objContentTemplateContainer = updatePanel.ContentTemplateContainer;

            for (int i = 0; i <= objControl.Parent.Controls.Count - 1; i++)
            {
                //find offset of original control
                if (objControl.Parent.Controls[i].ID == objControl.ID)
                {
                    //if ID matches
                    objControl.Parent.Controls.AddAt(i, updatePanel);
                    //insert update panel in that position
                    objContentTemplateContainer.Controls.Add(objControl);
                    //inject passed in control into update panel
                    break;
                }
            }

            if (blnIncludeProgress)
            {
                //create image for update progress control
                var objImage = new Image();
                objImage.ImageUrl = "~/images/progressbar.gif";
                //hardcoded
                objImage.AlternateText = "ProgressBar";

                var updateProgress = new UpdateProgress();
                updateProgress.AssociatedUpdatePanelID = updatePanel.ID;
                updateProgress.ID = updatePanel.ID + "_Prog";
                updateProgress.ProgressTemplate = new LiteralTemplate(objImage);

                objContentTemplateContainer.Controls.Add(updateProgress);
            }

            return updatePanel;
        }

        #endregion

        #region "Obsolete Methods"

        [Obsolete("Deprecated in DNN 5.4, Developers can work directly with the UpdatePanel")]
        public static Control ContentTemplateContainerControl(object objUpdatePanel)
        {
            return (objUpdatePanel as UpdatePanel)?.ContentTemplateContainer;
        }

        [Obsolete("Deprecated in DNN 5.4, MS AJax is now required for DotNetNuke 5.0. Develoers can create the control directly")]
        public static Control CreateUpdatePanelControl()
        {
            var updatePanel = new UpdatePanel();
            updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
            return updatePanel;
        }

        [Obsolete("Deprecated in DNN 5.4, MS AJax is now required for DotNetNuke 5.0. Developers can work directly with the UpdateProgress")]
        public static Control CreateUpdateProgressControl(string AssociatedUpdatePanelID)
        {
            var updateProgress = new UpdateProgress();
            updateProgress.ID = AssociatedUpdatePanelID + "_Prog";
            updateProgress.AssociatedUpdatePanelID = AssociatedUpdatePanelID;
            return updateProgress;
        }

        [Obsolete("Deprecated in DNN 5.4, MS AJax is now required for DotNetNuke 5.0. Developers can work directly with the UpdateProgress")]
        public static Control CreateUpdateProgressControl(string AssociatedUpdatePanelID, string ProgressHTML)
        {
            var updateProgress = new UpdateProgress();
            updateProgress.ID = AssociatedUpdatePanelID + "_Prog";
            updateProgress.AssociatedUpdatePanelID = AssociatedUpdatePanelID;
            updateProgress.ProgressTemplate = new LiteralTemplate(ProgressHTML);
            return updateProgress;
        }

        [Obsolete("Deprecated in DNN 5.4, MS AJax is now required for DotNetNuke 5.0. Developers can work directly with the UpdateProgress")]
        public static Control CreateUpdateProgressControl(string AssociatedUpdatePanelID, Control ProgressControl)
        {
            var updateProgress = new UpdateProgress();
            updateProgress.ID = AssociatedUpdatePanelID + "_Prog";
            updateProgress.AssociatedUpdatePanelID = AssociatedUpdatePanelID;
            updateProgress.ProgressTemplate = new LiteralTemplate(ProgressControl);
            return updateProgress;
        }

        [Obsolete("Deprecated in DNN 5.0, MS AJax is now required for DotNetNuke 5.0 and above - value no longer read from Host.EnableAjax")]
        public static bool IsHostEnabled()
        {
            return true;
        }

        [Obsolete("Deprecated in DNN 5.4, Replaced by GetScriptManager")]
        public static Control ScriptManagerControl(Page objPage)
        {
            return objPage.FindControl("ScriptManager");
        }

        [Obsolete("Deprecated in DNN 5.4, Developers can work directly with the ScriptManager")]
        public static void SetScriptManagerProperty(Page objPage, string PropertyName, object[] Args)
        {
            ScriptManager scriptManager = GetScriptManager(objPage);
            if (scriptManager != null)
            {
                Reflection.SetProperty(scriptManager.GetType(), PropertyName, scriptManager, Args);
            }
        }

        #endregion
    }
}
