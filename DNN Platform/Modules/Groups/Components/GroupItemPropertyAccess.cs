﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Entities.Groups;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Tokens;

    public class GroupItemTokenReplace : Services.Tokens.BaseCustomTokenReplace
    {
        public GroupItemTokenReplace(RoleInfo groupInfo)
        {
            this.PropertySource["groupitem"] = groupInfo;
        }

        public string ReplaceGroupItemTokens(string source)
        {
            return this.ReplaceTokens(source);
        }
    }
}
