// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Entities.Users.Social.Internal;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
    public class FollowersController : ServiceLocator<IFollowersController, FollowersController>
    {
        protected override Func<IFollowersController> GetFactory()
        {
            return () => new FollowersControllerImpl();
        }
    }
}
