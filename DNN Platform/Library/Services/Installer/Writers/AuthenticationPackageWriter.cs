#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Xml;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class AuthenticationPackageWriter : PackageWriterBase
    {
		#region "Constructors"
		
        public AuthenticationPackageWriter(PackageInfo package) : base(package)
        {
            AuthSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            Initialize();
        }

        public AuthenticationPackageWriter(AuthenticationInfo authSystem, PackageInfo package) : base(package)
        {
            AuthSystem = authSystem;
            Initialize();
        }
		
		#endregion

		#region "Public Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated Authentication System
		/// </summary>
		/// <value>An AuthenticationInfo object</value>
		/// -----------------------------------------------------------------------------
        public AuthenticationInfo AuthSystem { get; set; }
		
		#endregion

		#region "Private Methods"

        private void Initialize()
        {
            BasePath = Path.Combine("DesktopModules\\AuthenticationServices", AuthSystem.AuthenticationType);
            AppCodePath = Path.Combine("App_Code\\AuthenticationServices", AuthSystem.AuthenticationType);
            AssemblyPath = "bin";
        }

        private void WriteAuthenticationComponent(XmlWriter writer)
        {
			//Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "AuthenticationSystem");

            //Start authenticationService Element
            writer.WriteStartElement("authenticationService");

            writer.WriteElementString("type", AuthSystem.AuthenticationType);
            writer.WriteElementString("settingsControlSrc", AuthSystem.SettingsControlSrc);
            writer.WriteElementString("loginControlSrc", AuthSystem.LoginControlSrc);
            writer.WriteElementString("logoffControlSrc", AuthSystem.LogoffControlSrc);

            //End authenticationService Element
            writer.WriteEndElement();

            //End component Element
            writer.WriteEndElement();
        }
		
		#endregion

        protected override void WriteManifestComponent(XmlWriter writer)
        {
			//Write Authentication Component
            WriteAuthenticationComponent(writer);
        }
    }
}
