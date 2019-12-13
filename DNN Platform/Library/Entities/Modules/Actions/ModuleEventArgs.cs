using System;

namespace DotNetNuke.Entities.Modules.Actions
{
    public class ModuleEventArgs : EventArgs
    {
        public ModuleInfo Module { get; internal set; }
    }
}
