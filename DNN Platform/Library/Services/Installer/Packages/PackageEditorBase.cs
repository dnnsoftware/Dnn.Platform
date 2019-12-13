#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageEditorBase class provides a Base Classs for Package Editors
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PackageEditorBase : ModuleUserControlBase, IPackageEditor
    {
        private bool _IsWizard = Null.NullBoolean;
        private PackageInfo _Package;
        private int _PackageID = Null.NullInteger;

        protected string DisplayMode => (Request.QueryString["Display"] ?? "").ToLowerInvariant();

        protected virtual string EditorID
        {
            get
            {
                return Null.NullString;
            }
        }

        protected bool IsSuperTab
        {
            get
            {
                return ModuleContext.PortalSettings.ActiveTab.IsSuperTab;
            }
        }

        protected PackageInfo Package
        {
            get
            {
                if (_Package == null)
                {
                    _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.PackageID == PackageID); ;
                }
                return _Package;
            }
        }

        #region IPackageEditor Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Package ID
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int PackageID
        {
            get
            {
                return _PackageID;
            }
            set
            {
                _PackageID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Editor is in the Wizard
        /// </summary>
        /// <value>An Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool IsWizard
        {
            get
            {
                return _IsWizard;
            }
            set
            {
                _IsWizard = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Used to Initialize the Control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public virtual void Initialize()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Used to Update the Package
        /// </summary>
        /// -----------------------------------------------------------------------------
        public virtual void UpdatePackage()
        {
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            ID = EditorID;
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
