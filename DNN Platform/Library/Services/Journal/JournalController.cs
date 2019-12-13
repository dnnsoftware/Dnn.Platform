// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Journal
{
    public class JournalController : ServiceLocator<IJournalController, JournalController>
    {
        protected override Func<IJournalController> GetFactory()
        {
            return () => new JournalControllerImpl();
        }
    }
}
