// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PagesController : ServiceLocator<IPagesController, PagesController>
    {
        protected override Func<IPagesController> GetFactory()
        {
            return () => new PagesControllerImpl();
        }
    }
}
