
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System.IO;
using System.Xml;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;

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
        public AuthenticationPackageWriter(PackageInfo package)
            : base(package)
        {
            this.AuthSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            this.Initialize();
        }

        public AuthenticationPackageWriter(AuthenticationInfo authSystem, PackageInfo package)
            : base(package)
        {
            this.AuthSystem = authSystem;
            this.Initialize();
        }



        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the associated Authentication System
        /// </summary>
        /// <value>An AuthenticationInfo object</value>
        /// -----------------------------------------------------------------------------
        public AuthenticationInfo AuthSystem { get; set; }

        private void Initialize()
        {
            this.BasePath = Path.Combine("DesktopModules\\AuthenticationServices", this.AuthSystem.AuthenticationType);
            this.AppCodePath = Path.Combine("App_Code\\AuthenticationServices", this.AuthSystem.AuthenticationType);
            this.AssemblyPath = "bin";
        }

        private void WriteAuthenticationComponent(XmlWriter writer)
        {
            // Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "AuthenticationSystem");

            // Start authenticationService Element
            writer.WriteStartElement("authenticationService");

            writer.WriteElementString("type", this.AuthSystem.AuthenticationType);
            writer.WriteElementString("settingsControlSrc", this.AuthSystem.SettingsControlSrc);
            writer.WriteElementString("loginControlSrc", this.AuthSystem.LoginControlSrc);
            writer.WriteElementString("logoffControlSrc", this.AuthSystem.LogoffControlSrc);

            // End authenticationService Element
            writer.WriteEndElement();

            // End component Element
            writer.WriteEndElement();
        }

        protected override void WriteManifestComponent(XmlWriter writer)
        {
            // Write Authentication Component
            this.WriteAuthenticationComponent(writer);
        }
    }
}
