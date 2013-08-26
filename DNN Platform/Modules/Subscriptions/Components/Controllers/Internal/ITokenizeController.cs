#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Entities.Users;
using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public interface ITokenizeController
    {
        /// <summary>
        /// Turn a <see cref="InstantNotification" /> object into a formatted notification based on some templating settings.
        /// </summary>
        FormattedNotification Tokenize(UserInfo author, InstantNotification instantNotification);

        /// <summary>
        /// Turn a <see cref="DigestNotification" /> object into a formatted notification based on some templating settings.
        /// </summary>
        FormattedNotification Tokenize(DigestNotification digestNotification);
    }
}