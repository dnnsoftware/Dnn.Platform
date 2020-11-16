// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class    : ModuleControlBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleControlBase is a base class for Module Controls that inherits from the
    /// Control base class.  As with all MontrolControl base classes it implements
    /// IModuleControl.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ModuleControlBase : Control, IModuleControl
    {
        private string _localResourceFile;
        private ModuleInstanceContext _moduleContext;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the underlying base control for this ModuleControl.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public Control Control
        {
            get
            {
                return this;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this control (used primarily for UserControls).
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ControlPath
        {
            get
            {
                return this.TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name for this control.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ControlName
        {
            get
            {
                return this.GetType().Name.Replace("_", ".");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Module Context for this control.
        /// </summary>
        /// <returns>A ModuleInstanceContext.</returns>
        /// -----------------------------------------------------------------------------
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this._moduleContext == null)
                {
                    this._moduleContext = new ModuleInstanceContext(this);
                }

                return this._moduleContext;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the local resource file for this control.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    fileRoot = this.ControlPath + "/" + Localization.LocalResourceDirectory + "/" + this.ID;
                }
                else
                {
                    fileRoot = this._localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this._localResourceFile = value;
            }
        }
    }
}
