﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;

    [Serializable]
    public class ParameterRedirectAction
    {
        public string Action { get; set; }

        public bool ChangeToSiteRoot { get; set; }

        public bool ForDefaultPage { get; set; }

        public string LookFor { get; set; }

        public string Name { get; set; }

        public int PortalId { get; set; }

        public string RedirectTo { get; set; }

        public int TabId { get; set; }
    }
}
