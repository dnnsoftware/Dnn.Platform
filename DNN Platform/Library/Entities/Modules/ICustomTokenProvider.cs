// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.UI;
using System.Collections.Generic;
using DotNetNuke.UI.Modules;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Entities.Modules
{
    public interface ICustomTokenProvider
    {
        IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext);
    }
}
