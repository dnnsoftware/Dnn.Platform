// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.NavigationProvider;

using System.Diagnostics.CodeAnalysis;

using DotNetNuke.UI.WebControls;

public class NavigationEventArgs
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ID;

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public DNNNode Node;

    /// <summary>Initializes a new instance of the <see cref="NavigationEventArgs"/> class.</summary>
    /// <param name="strID"></param>
    /// <param name="objNode"></param>
    public NavigationEventArgs(string strID, DNNNode objNode)
    {
        this.ID = strID;
        this.Node = objNode;
    }
}
