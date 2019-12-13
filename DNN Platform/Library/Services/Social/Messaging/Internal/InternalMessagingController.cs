// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
