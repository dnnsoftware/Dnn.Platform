// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    using DotNetNuke.Entities.Tabs;

    /// <summary>Provides the ability to manage links in the Persona Bar menu.</summary>
    public interface IAdminMenuController
    {
        /// <summary>Create a link for the given page.</summary>
        /// <param name="tab">The page for which to create a link in the Persona Bar.</param>
        void CreateLinkMenu(TabInfo tab);

        /// <summary>Delete the link for the given page.</summary>
        /// <param name="tab">The page for which to delete the link in the Persona Bar.</param>
        void DeleteLinkMenu(TabInfo tab);
    }
}
