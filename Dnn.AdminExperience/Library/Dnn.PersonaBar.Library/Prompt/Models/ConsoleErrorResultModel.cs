using System.IO;
using Localization = DotNetNuke.Services.Localization.Localization;

namespace Dnn.PersonaBar.Library.Prompt.Models
{
    public class ConsoleErrorResultModel : ConsoleResultModel
    {
        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        public ConsoleErrorResultModel()
        {
            IsError = true;
            Output = Localization.GetString("Prompt_InvalidSyntax", LocalResourcesFile, true);
        }

        public ConsoleErrorResultModel(string errMessage)
        {
            IsError = true;
            Output = errMessage;
        }
    }
}