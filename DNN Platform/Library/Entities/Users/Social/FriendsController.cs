#region Usings

using System;

using DotNetNuke.Entities.Users.Social.Internal;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
    public class FriendsController : ServiceLocator<IFriendsController, FriendsController>
    {
        protected override Func<IFriendsController> GetFactory()
        {
            return () => new FriendsControllerImpl();
        }
    }
}
