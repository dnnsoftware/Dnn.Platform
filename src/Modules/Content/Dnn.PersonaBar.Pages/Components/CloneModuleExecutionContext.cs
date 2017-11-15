using System;
using System.Threading;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Pages.Components
{
    public class CloneModuleExecutionContext : ServiceLocator<ICloneModuleExecutionContext, CloneModuleExecutionContext>, ICloneModuleExecutionContext
    {
        private const string CloneModuleSlotName = "CloneModuleContext";

        protected override Func<ICloneModuleExecutionContext> GetFactory()
        {
            return () => new CloneModuleExecutionContext();
        }

        public void SetCloneModuleContext(bool cloneModule)
        {
            var slot = Thread.GetNamedDataSlot(CloneModuleSlotName);
            Thread.SetData(slot, cloneModule ? bool.TrueString : bool.FalseString);
        }
    }
}