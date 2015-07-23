#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.IO;
using System.Web.UI;

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : ModuleUserControlBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleUserControlBase is a base class for Module Controls that inherits from the
    /// UserControl base class.  As with all MontrolControl base classes it implements
    /// IModuleControl.
    /// </summary>
    /// <history>
    /// 	[cnurse]	12/16/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class ModuleUserControlBase : UserControl, IModuleControl
    {
        private string _localResourceFile;
        private ModuleInstanceContext _moduleContext;

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

		protected string LocalizeSafeJsString(string key)
		{
			return Localization.GetSafeJSString(key, LocalResourceFile);
		}

        #region IModuleControl Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the underlying base control for this ModuleControl
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	12/17/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	12/16/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	12/16/2007  created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	12/16/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = Path.Combine(ControlPath, Localization.LocalResourceDirectory + "/" + ID);
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
        /// <history>
        /// 	[cnurse]	12/16/2007  created
        /// </history>
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
