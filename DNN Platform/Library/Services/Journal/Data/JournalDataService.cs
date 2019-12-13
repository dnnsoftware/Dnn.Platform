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
