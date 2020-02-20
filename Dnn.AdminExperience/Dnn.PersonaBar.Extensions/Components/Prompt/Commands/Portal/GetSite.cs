// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    /// <summary>
    /// alias for get-portal
    /// </summary>
    [ConsoleCommand("get-site", Constants.PortalCategory, "Prompt_GetSite_Description")]
    public class GetSite : GetPortal
    {
    }
}
