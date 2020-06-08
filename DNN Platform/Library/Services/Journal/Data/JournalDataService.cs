﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.IO;
using System.Xml;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Journal {
    public class JournalDataService : ServiceLocator<IJournalDataService, JournalDataService>
    {
        protected override Func<IJournalDataService> GetFactory()
        {
            return () => new JournalDataServiceImpl();
        }
    }
}
