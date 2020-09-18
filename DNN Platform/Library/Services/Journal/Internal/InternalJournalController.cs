// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal.Internal
{
    using System;

    using DotNetNuke.Framework;

    public class InternalJournalController : ServiceLocator<IInternalJournalController, InternalJournalController>
    {
        protected override Func<IInternalJournalController> GetFactory()
        {
            return () => new InternalJournalControllerImpl();
        }
    }
}
