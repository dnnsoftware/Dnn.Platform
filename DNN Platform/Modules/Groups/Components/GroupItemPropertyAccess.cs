// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            PropertySource["groupitem"] = groupInfo;
        }
        public string ReplaceGroupItemTokens(string source) {
            return base.ReplaceTokens(source);
        }
    }
}
