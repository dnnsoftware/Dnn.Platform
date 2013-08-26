#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.
#endregion

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.SubscriptionsMgmt.Components.Common;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers
{
    public class BusinessController : IUpgradeable
    {
        #region IUpgradeable

        /// <summary>Implements the IUpgradeable Interface</summary>
        public string UpgradeModule(string version)
        {
            var message = string.Empty;

            switch (version)
            {
                case "01.00.00":
                    //Integration.Mechanics.Instance.AddScoringDefinitions();
                    //message += "Added scoring definitions for the Subscription Management module. " + Environment.NewLine;

                    //SocialLibrary.Components.Common.Utilities.CategorizeSocialModule(DesktopModuleController.GetDesktopModuleByFriendlyName(Constants.DesktopModuleFriendlyName));
                   // message += "Added Subscription Management module to Social module category. " + Environment.NewLine;

                    break;
            }

            return message;
        }

        #endregion
    }
}
