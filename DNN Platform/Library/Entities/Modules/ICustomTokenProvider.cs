// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
