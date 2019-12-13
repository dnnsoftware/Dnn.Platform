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
