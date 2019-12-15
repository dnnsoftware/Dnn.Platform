// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public class UrlRewriterUtilsWrapper : IUrlRewriterUtilsWrapper
    {        
        public FriendlyUrlOptions GetExtendOptionsForURLs(int portalId)
        {
            return UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalId)));
        }
    }
}
