
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Social.Data;
using DotNetNuke.Entities.Users.Social.Internal;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Entities.Users.Social
{
    /// <summary>
    /// Business Layer to manage Relationships. Also contains CRUD methods.
    /// </summary>
    public class RelationshipController : ServiceLocator<IRelationshipController, RelationshipController>
    {
        protected override Func<IRelationshipController> GetFactory()
        {
            return () => new RelationshipControllerImpl();
        }
    }
}
