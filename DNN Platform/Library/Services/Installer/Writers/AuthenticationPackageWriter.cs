// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
