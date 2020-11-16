// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using Dnn.PersonaBar.Library.Prompt.Attributes;

    /// <summary>
    /// alias for get-portal.
    /// </summary>
    [ConsoleCommand("get-site", Constants.PortalCategory, "Prompt_GetSite_Description")]
    public class GetSite : GetPortal
    {}
}
