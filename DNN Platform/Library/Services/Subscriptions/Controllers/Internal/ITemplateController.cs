#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    /// <summary>
    /// <c>ITemplateController</c> converts a <see cref="InstantNotification" /> or <see cref="DigestNotification" />
    /// into a <see cref="FormattedNotification" /> object that is ready to be sent to an end-user.
    /// </summary>
    public interface ITemplateController
    {
        /// <summary>
        /// Convert an <see cref="InstantNotification" /> object into a formatted notification, <see cref="FormattedNotification" />.
        /// </summary>
        FormattedNotification Format(InstantNotification instantNotification);

        /// <summary>
        /// Convert an <see cref="DigestNotification" /> object into a formatted notification, <see cref="FormattedNotification" />.
        /// </summary>
        FormattedNotification Format(DigestNotification digestNotification);
    }
}