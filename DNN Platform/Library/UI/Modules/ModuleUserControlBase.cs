// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules;

using System.IO;
using System.Web.UI;

using DotNetNuke.Services.Localization;

/// <summary>
/// ModuleUserControlBase is a base class for Module Controls that inherits from the
/// UserControl base class.  As with all MontrolControl base classes it implements
/// IModuleControl.
/// </summary>
public class ModuleUserControlBase : UserControl, IModuleControl
{
    private string localResourceFile;
    private ModuleInstanceContext moduleContext;

    /// <summary>Gets the underlying base control for this ModuleControl.</summary>
    /// <returns>A String.</returns>
    public Control Control
    {
        get
        {
            return this;
        }
    }

    /// <summary>Gets the Path for this control (used primarily for UserControls).</summary>
    /// <returns>A String.</returns>
    public string ControlPath
    {
        get
        {
            return this.TemplateSourceDirectory + "/";
        }
    }

    /// <summary>Gets the Name for this control.</summary>
    /// <returns>A String.</returns>
    public string ControlName
    {
        get
        {
            return this.GetType().Name.Replace("_", ".");
        }
    }

    /// <summary>Gets the Module Context for this control.</summary>
    /// <returns>A ModuleInstanceContext.</returns>
    public ModuleInstanceContext ModuleContext
    {
        get
        {
            if (this.moduleContext == null)
            {
                this.moduleContext = new ModuleInstanceContext(this);
            }

            return this.moduleContext;
        }
    }

    /// <summary>Gets or sets the local resource file for this control.</summary>
    /// <returns>A String.</returns>
    public string LocalResourceFile
    {
        get
        {
            string fileRoot;
            if (string.IsNullOrEmpty(this.localResourceFile))
            {
                fileRoot = Path.Combine(this.ControlPath, Localization.LocalResourceDirectory + "/" + this.ID);
            }
            else
            {
                fileRoot = this.localResourceFile;
            }

            return fileRoot;
        }

        set
        {
            this.localResourceFile = value;
        }
    }

    /// <summary>Gets a localized string value from a key.</summary>
    /// <param name="key">The key to find the localized value.</param>
    /// <returns>The localized value.</returns>
    protected string LocalizeString(string key)
    {
        return Localization.GetString(key, this.LocalResourceFile);
    }

    /// <summary>Gets a localized string with special characters escape for safe use in javascript.</summary>
    /// <param name="key">The key to find the localized value.</param>
    /// <returns>The localized text cleaned up for javascript usage.</returns>
    protected string LocalizeSafeJsString(string key)
    {
        return Localization.GetSafeJSString(key, this.LocalResourceFile);
    }
}
