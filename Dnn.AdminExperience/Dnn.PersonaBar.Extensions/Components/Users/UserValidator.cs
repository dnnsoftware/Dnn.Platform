// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components;

using System.Collections.Generic;
using System.Net;

using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

public class UserValidator : IUserValidator
{
    private IPortalController portalController;
    private IUserControllerWrapper userControllerWrapper;
    private IContentVerifier contentVerifier;

    /// <summary>Initializes a new instance of the <see cref="UserValidator"/> class.</summary>
    public UserValidator()
        : this(PortalController.Instance, new UserControllerWrapper(), new ContentVerifier())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UserValidator"/> class.</summary>
    /// <param name="portalController"></param>
    /// <param name="userControllerWrapper"></param>
    /// <param name="contentVerifier"></param>
    public UserValidator(IPortalController portalController, IUserControllerWrapper userControllerWrapper, IContentVerifier contentVerifier)
    {
        this.portalController = portalController;
        this.userControllerWrapper = userControllerWrapper;
        this.contentVerifier = contentVerifier;
    }

    /// <inheritdoc/>
    public ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo currentUserInfo, out UserInfo userInfo)
    {
        userInfo = null;
        if (!userId.HasValue)
        {
            return new ConsoleErrorResultModel(Localization.GetString("Prompt_NoUserId", Constants.LocalResourcesFile));
        }

        KeyValuePair<HttpStatusCode, string> response;
        userInfo = this.userControllerWrapper.GetUser(userId.Value, portalSettings, currentUserInfo, out response);

        if (userInfo == null)
        {
            var portals = this.portalController.GetPortals();

            foreach (var portal in portals)
            {
                var portalInfo = portal as PortalInfo;
                userInfo = this.userControllerWrapper.GetUserById(portalInfo.PortalID, userId.Value);

                if (userInfo != null)
                {
                    break;
                }
            }

            if (userInfo != null &&
                !this.contentVerifier.IsContentExistsForRequestedPortal(
                    userInfo.PortalID,
                    portalSettings,
                    true))
            {
                userInfo = null;
            }
        }

        return userInfo == null ? new ConsoleErrorResultModel(response.Value) : null;
    }
}
