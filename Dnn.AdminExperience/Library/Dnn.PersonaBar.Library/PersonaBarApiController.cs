// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library
{
    /// <summary>
    /// The base class for persona bar api.
    /// </summary>
    public abstract class PersonaBarApiController : DnnApiController
    {
        public int PortalId => PortalSettings.PortalId;
    }
}
