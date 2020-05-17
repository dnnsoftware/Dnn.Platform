﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

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
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

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
