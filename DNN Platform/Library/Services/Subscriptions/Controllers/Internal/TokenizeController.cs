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
    public class TokenizeController : ServiceLocator<ITokenizeController, TokenizeController>
    {
        protected override Func<ITokenizeController> GetFactory()
        {
            return () => new TokenizeControllerImpl();
        }
    }
}