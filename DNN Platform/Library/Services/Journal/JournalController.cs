// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
