// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Contract for XML file steps.</summary>
    internal interface IXmlStep : IStep
    {
        /// <summary>Gets or sets the path of the XML file to load, relative to the application root path.</summary>
        string RelativeFilePath { get; set; }
    }
}
