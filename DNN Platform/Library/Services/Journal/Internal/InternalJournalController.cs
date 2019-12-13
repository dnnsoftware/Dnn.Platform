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
