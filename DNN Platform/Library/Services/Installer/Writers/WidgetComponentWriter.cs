﻿#region Usings

using System.Collections.Generic;

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The WidgetComponentWriter class handles creating the manifest for Widget Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class WidgetComponentWriter : FileComponentWriter
    {
        public WidgetComponentWriter(string basePath, Dictionary<string, InstallFile> files, PackageInfo package) : base(basePath, files, package)
        {
        }

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
        /// Gets the name of the Component Type ("Widget")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                return "Widget";
            }
        }
    }
}
