#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
