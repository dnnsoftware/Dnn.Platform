using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;

namespace Dnn.EditBar.UI.Components
{
    public class BusinessController : IUpgradeable
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(BusinessController));

        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    break;
            }

            return "Success";
        }
    }
}
