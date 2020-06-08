// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// Alias for list-portals
    /// </summary>
    [ConsoleCommand("list-sites", Constants.PortalCategory, "Prompt_ListSites_Description")]
    public class ListSites : ListPortals
    {
    }
}
