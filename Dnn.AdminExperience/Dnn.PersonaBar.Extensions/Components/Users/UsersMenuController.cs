// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    public class UsersMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            return true;
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("userId", UserController.Instance.GetCurrentUserInfo().UserID);
            settings.Add("requiresQuestionAndAnswer", MembershipProviderConfig.RequiresQuestionAndAnswer);
            settings.Add("dataConsentActive", PortalSettings.Current.DataConsentActive);
            return settings;
        }
    }
}
