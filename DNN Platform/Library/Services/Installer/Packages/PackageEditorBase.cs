#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PackageEditorBase class provides a Base Classs for Package Editors
    /// </summary>
    /// <history>
    /// 	[cnurse]	02/04/2008	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class PackageEditorBase : ModuleUserControlBase, IPackageEditor
    {
        private bool _IsWizard = Null.NullBoolean;
        private PackageInfo _Package;
        private int _PackageID = Null.NullInteger;

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
        /// <history>
        /// 	[cnurse]	02/04/2008	created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	08/26/2008	created
        /// </history>
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
        /// <history>
        /// 	[cnurse]	02/21/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public virtual void Initialize()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Used to Update the Package
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/21/2008	created
        /// </history>
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
