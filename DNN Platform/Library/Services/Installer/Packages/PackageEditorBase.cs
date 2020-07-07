// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.Modules;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageEditorBase class provides a Base Classs for Package Editors.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PackageEditorBase : ModuleUserControlBase, IPackageEditor
    {
        private bool _IsWizard = Null.NullBoolean;
        private PackageInfo _Package;
        private int _PackageID = Null.NullInteger;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Package ID.
        /// </summary>
        /// <value>An Integer.</value>
        /// -----------------------------------------------------------------------------
        public int PackageID
        {
            get
            {
                return this._PackageID;
            }

            set
            {
                this._PackageID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Editor is in the Wizard.
        /// </summary>
        /// <value>An Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool IsWizard
        {
            get
            {
                return this._IsWizard;
            }

            set
            {
                this._IsWizard = value;
            }
        }

        protected string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

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
                return this.ModuleContext.PortalSettings.ActiveTab.IsSuperTab;
            }
        }

        protected PackageInfo Package
        {
            get
            {
                if (this._Package == null)
                {
                    this._Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.PackageID == this.PackageID);
                }

                return this._Package;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Used to Initialize the Control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public virtual void Initialize()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Used to Update the Package.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public virtual void UpdatePackage()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            this.ID = this.EditorID;
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
