// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>A contract specifying the ability to provide information about a ribbon bar tool.</summary>
    public interface IDnnRibbonBarTool
    {
        /// <summary>Gets or sets the tool name.</summary>
        string ToolName { get; set; }
    }
}
