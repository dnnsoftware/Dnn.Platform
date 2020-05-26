// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Urls
{
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
