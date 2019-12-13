#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Internal
{
	/// <summary>
	/// Business Layer to manage Messaging. Also contains CRUD methods.
	/// </summary>
    public class InternalMessagingController : ServiceLocator<IInternalMessagingController, InternalMessagingController>
    {
        protected override Func<IInternalMessagingController> GetFactory()
        {
            return () => new InternalMessagingControllerImpl();
        }
    }
}
