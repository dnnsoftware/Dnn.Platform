#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

using DotNetNuke.Framework;

namespace DotNetNuke.Subscriptions.Components.Controllers
{
    public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>
    {
        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionControllerImpl();
        }

    }
}