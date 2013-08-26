#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

using DotNetNuke.Framework;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public class SubscriptionTypeController : ServiceLocator<ISubscriptionTypeController, SubscriptionTypeController>
    {
        protected override Func<ISubscriptionTypeController> GetFactory()
        {
            return () => new SubscriptionTypeControllerImpl();
        }
    }
}