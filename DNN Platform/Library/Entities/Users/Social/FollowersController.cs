// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
