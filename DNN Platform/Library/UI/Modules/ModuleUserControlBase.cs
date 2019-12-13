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
