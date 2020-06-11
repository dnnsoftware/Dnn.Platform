// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Xml.XPath;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TypeDependency determines whether the dependent type is installed
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class TypeDependency : DependencyBase
    {
        private string _missingDependentType = String.Empty;
        private string _dependentTypes;

        public override string ErrorMessage
        {
            get
            {
                return Util.INSTALL_Namespace + " - " + this._missingDependentType;
            }
        }

        public override bool IsValid
        {
            get
            {
                bool isValid = true;
                if (!String.IsNullOrEmpty(this._dependentTypes))
                {
                    foreach (string dependentType in (this._dependentTypes + ";").Split(';'))
                    {
                        if (!String.IsNullOrEmpty(dependentType.Trim()))
                        {
                            if (Reflection.CreateType(dependentType, true) == null)
                            {
                                this._missingDependentType = dependentType;
                                isValid = false;
                                break;
                            }
                        }
                    }
                }
                return isValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            this._dependentTypes = dependencyNav.Value;
        }
    }
}
