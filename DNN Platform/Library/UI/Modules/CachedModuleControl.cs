﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : CachedModuleControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CachedModuleControl represents a cached "ModuleControl".  It inherits from
    /// Literal and implements the IModuleControl interface
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CachedModuleControl : Literal, IModuleControl
    {
        private string _localResourceFile;
        private ModuleInstanceContext _moduleContext;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CachedModuleControl
        /// </summary>
        /// <param name="cachedContent">The cached Content for this control</param>
        /// -----------------------------------------------------------------------------
        public CachedModuleControl(string cachedContent)
        {
            Text = cachedContent;
        }

        #region IModuleControl Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the underlying base control for this ModuleControl
        /// </summary>
        /// <returns>A String</returns>
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
        /// Gets the Path for this control (used primarily for UserControls)
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlPath
        {
            get
            {
                return TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name for this control
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlName
        {
            get
            {
                return GetType().Name.Replace("_", ".");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the local resource file for this control
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;

                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = ControlPath + "/" + Localization.LocalResourceDirectory + "/" + ID;
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Module Context for this control
        /// </summary>
        /// <returns>A ModuleInstanceContext</returns>
        /// -----------------------------------------------------------------------------
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (_moduleContext == null)
                {
                    _moduleContext = new ModuleInstanceContext(this);
                }
                return _moduleContext;
            }
        }

        #endregion
    }
}
