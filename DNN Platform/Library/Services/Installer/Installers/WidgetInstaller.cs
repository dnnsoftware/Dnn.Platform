// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.IO;

using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The WidgetInstaller installs Widget Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class WidgetInstaller : FileInstaller
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("widgetFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "widgetFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("widgetFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "widgetFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PhysicalBasePath for the widget files
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string PhysicalBasePath
        {
            get
            {
                string widgetPath = Path.Combine("Resources\\Widgets\\User", BasePath);
                return Path.Combine(Globals.ApplicationMapPath, widgetPath);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "js";
            }
        }
    }
}
