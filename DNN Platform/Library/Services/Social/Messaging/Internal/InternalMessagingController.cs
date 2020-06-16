// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal
{
    using System;

    using DotNetNuke.Framework;

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
