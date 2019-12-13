// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                return Util.INSTALL_Namespace + " - " + _missingDependentType;
            }
        }

        public override bool IsValid
        {
            get
            {
                bool isValid = true;
                if (!String.IsNullOrEmpty(_dependentTypes))
                {
                    foreach (string dependentType in (_dependentTypes + ";").Split(';'))
                    {
                        if (!String.IsNullOrEmpty(dependentType.Trim()))
                        {
                            if (Reflection.CreateType(dependentType, true) == null)
                            {
                                _missingDependentType = dependentType;
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
            _dependentTypes = dependencyNav.Value;
        }
    }
}
