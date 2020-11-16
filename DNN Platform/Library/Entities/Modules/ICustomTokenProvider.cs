// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules;

    public interface ICustomTokenProvider
    {
        IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext);
    }
}
