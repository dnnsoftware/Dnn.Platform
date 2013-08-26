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
    public class TemplateController : ServiceLocator<ITemplateController, TemplateController>
    {
        protected override Func<ITemplateController> GetFactory()
        {
            return () => new TemplateControllerImpl();
        }
    }
}