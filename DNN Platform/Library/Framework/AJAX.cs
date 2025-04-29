// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Host;
using DotNetNuke.UI.WebControls;

public class AJAX
{
    /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
    public static void AddScriptManager(Page page)
    {
        AddScriptManager(page, true);
    }

    /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
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
                    using (var scriptManager = new ScriptManager // RadScriptManager
                           {
                               ID = "ScriptManager",
                               EnableScriptGlobalization = true,
                               SupportsPartialRendering = true,
                           })
                    {
                        if (checkCdn)
                        {
                            scriptManager.EnableCdn = Host.EnableMsAjaxCdn;
                            scriptManager.EnableCdnFallback = Host.EnableMsAjaxCdn;
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
                    // suppress error adding script manager to support edge-case of module developers custom aspx pages that inherit from basepage and use code blocks
                }
            }
        }
    }

    /// <summary>Gets the current ScriptManager on the page.</summary>
    /// <param name="objPage">the page instance.</param>
    /// <returns>The ScriptManager instance, or <c>null</c>.</returns>
    public static ScriptManager GetScriptManager(Page objPage)
    {
        return objPage.FindControl("ScriptManager") as ScriptManager;
    }

    /// <summary>IsEnabled can be used to determine if AJAX has been enabled already as we only need one Script Manager per page.</summary>
    /// <returns><see langword="true"/> if the current request has enabled the script manager, otherwise <see langword="false"/>.</returns>
    public static bool IsEnabled()
    {
        if (HttpContext.Current.Items["System.Web.UI.ScriptManager"] == null)
        {
            return false;
        }
        else
        {
            return (bool)HttpContext.Current.Items["System.Web.UI.ScriptManager"];
        }
    }

    /// <summary>IsInstalled can be used to determine if AJAX is installed on the server.</summary>
    /// <returns><see langword="true"/> if ASP.NET AJAX is installed (always), otherwise <see langword="false"/> (never).</returns>
    public static bool IsInstalled()
    {
        return true;
    }

    /// <summary>Allows a control to be excluded from UpdatePanel async callback.</summary>
    public static void RegisterPostBackControl(Control objControl)
    {
        ScriptManager objScriptManager = GetScriptManager(objControl.Page);
        if (objScriptManager != null)
        {
            objScriptManager.RegisterPostBackControl(objControl);
        }
    }

    /// <summary>RegisterScriptManager must be used by developers to instruct the framework that AJAX is required on the page.</summary>
    public static void RegisterScriptManager()
    {
        if (!IsEnabled())
        {
            HttpContext.Current.Items.Add("System.Web.UI.ScriptManager", true);
        }
    }

    /// <summary>RemoveScriptManager will remove the ScriptManager control during Page Render if the RegisterScriptManager has not been called.</summary>
    public static void RemoveScriptManager(Page objPage)
    {
        if (!IsEnabled())
        {
            Control objControl = objPage.FindControl("ScriptManager");
            if (objControl != null)
            {
                objPage.Form.Controls.Remove(objControl);
            }
        }
    }

    /// <summary>Wraps a control in an update panel.</summary>
    /// <returns>The <see cref="UpdatePanel"/>.</returns>
    public static Control WrapUpdatePanelControl(Control objControl, bool blnIncludeProgress)
    {
        var updatePanel = new UpdatePanel();
        updatePanel.ID = objControl.ID + "_UP";
        updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

        Control objContentTemplateContainer = updatePanel.ContentTemplateContainer;

        for (int i = 0; i <= objControl.Parent.Controls.Count - 1; i++)
        {
            // find offset of original control
            if (objControl.Parent.Controls[i].ID == objControl.ID)
            {
                // if ID matches
                objControl.Parent.Controls.AddAt(i, updatePanel);

                // insert update panel in that position
                objContentTemplateContainer.Controls.Add(objControl);

                // inject passed in control into update panel
                break;
            }
        }

        if (blnIncludeProgress)
        {
            // create image for update progress control
            var objImage = new Image();
            objImage.ImageUrl = "~/images/progressbar.gif";

            // hardcoded
            objImage.AlternateText = "ProgressBar";

            var updateProgress = new UpdateProgress();
            updateProgress.AssociatedUpdatePanelID = updatePanel.ID;
            updateProgress.ID = updatePanel.ID + "_Prog";
            updateProgress.ProgressTemplate = new LiteralTemplate(objImage);

            objContentTemplateContainer.Controls.Add(updateProgress);
        }

        return updatePanel;
    }
}
