using System;

namespace DotNetNuke.UI.Modules
{
    public sealed class ModuleControlPipeline
    {
        private static readonly Lazy<IModuleControlPipeline> _instance = new Lazy<IModuleControlPipeline>(OnCreateInstance);

        private static IModuleControlPipeline OnCreateInstance()
        {
            var type = Framework.Reflection.CreateType("DotNetNuke.ModulePipeline.ModuleControlFactory");
            return Framework.Reflection.CreateObject(type) as IModuleControlPipeline;
        }

        public static IModuleControlPipeline Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
