using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Entities.Modules;

namespace Dnn.PersonaBar.Vocabularies.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.04.00":
                    UpdateMenuController();
                    break;
            }

            return "Success";
        }

        private void UpdateMenuController()
        {
            PersonaBarRepository.Instance.UpdateMenuController(Constants.MenuIdentifier, string.Empty);
        }
    }
}