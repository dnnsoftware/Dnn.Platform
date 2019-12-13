// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Framework;

namespace DotNetNuke.Services.Journal.Internal
{
    public class InternalJournalController : ServiceLocator<IInternalJournalController, InternalJournalController>
    {
        protected override Func<IInternalJournalController> GetFactory()
        {
            return () => new InternalJournalControllerImpl();
        }
    }
}
