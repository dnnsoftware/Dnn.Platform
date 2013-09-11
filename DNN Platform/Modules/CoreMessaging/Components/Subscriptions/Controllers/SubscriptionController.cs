// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers
{
    public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>
    {
        #region Overrides of ServiceLocator<ISubscriptionController,SubscriptionController>

        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionControllerImpl();
        }

        #endregion
    }
}