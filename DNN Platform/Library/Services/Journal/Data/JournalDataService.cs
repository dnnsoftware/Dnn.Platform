// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
