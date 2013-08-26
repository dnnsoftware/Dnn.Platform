#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

using DotNetNuke.Framework;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class PublishController : ServiceLocator<IPublishController, PublishController>
    {
        protected override Func<IPublishController> GetFactory()
        {
            return () => new PublishControllerImpl();
        }
    }
}