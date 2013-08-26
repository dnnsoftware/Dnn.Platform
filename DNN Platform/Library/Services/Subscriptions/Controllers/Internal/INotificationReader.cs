#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    /// <summary>
    /// This API is used to read enqueued notifications from the Subscription subsystem -- either instant or digest.
    /// </summary>
    public interface INotificationReader
    {
        /// <summary>
        /// Get all instant notifications newer than <paramref name="lowerBound"/>.
        /// </summary>
        IEnumerable<InstantNotification> GetInstantNotifications(int portalId, DateTime lowerBound);

        /// <summary>
        /// Get all combined, digest-style notifications newer than <paramref name="lowerBound" />.
        /// </summary>
        IEnumerable<DigestNotification> GetDigestNotifications(int portalId, Frequency frequency, DateTime lowerBound);
    }
}