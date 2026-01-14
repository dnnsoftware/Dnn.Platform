// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using DotNetNuke.UI.WebControls;

    using MenuNode = DotNetNuke.Web.DDRMenu.MenuNode;

    /// <summary>A contract specifying the ability to localize DDR menu nodes.</summary>
    public interface ILocaliser
    {
        /// <summary>Localizes the menu nodes.</summary>
        /// <param name="nodes">The collection of nodes to localize.</param>
        /// <returns>The localized collection of nodes.</returns>
        DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes);

        /// <summary>Localizes a single node.</summary>
        /// <param name="node">The node to localize.</param>
        /// <param name="portalId">The portal ID for which the node is being localized.</param>
        void LocaliseNode(MenuNode node, int portalId);
    }
}
