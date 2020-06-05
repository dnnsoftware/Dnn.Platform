﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library
{
    /// <summary>
    /// The base class for persona bar api.
    /// </summary>
    public abstract class PersonaBarApiController : DnnApiController
    {
        public int PortalId => this.PortalSettings.PortalId;
    }
}
