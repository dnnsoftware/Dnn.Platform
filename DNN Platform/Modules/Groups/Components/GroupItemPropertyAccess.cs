﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Services.Journal;
using DotNetNuke.Entities.Groups;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.Groups.Components {
    public class GroupItemTokenReplace : Services.Tokens.BaseCustomTokenReplace {
        public GroupItemTokenReplace(RoleInfo groupInfo)
        {
            this.PropertySource["groupitem"] = groupInfo;
        }
        public string ReplaceGroupItemTokens(string source) {
            return base.ReplaceTokens(source);
        }
    }
}
