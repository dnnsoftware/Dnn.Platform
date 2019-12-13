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
