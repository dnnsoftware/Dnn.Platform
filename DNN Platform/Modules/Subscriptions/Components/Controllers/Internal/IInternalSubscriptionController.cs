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
    public interface IInternalSubscriptionController
    {
        /// <summary>
        /// Get a list of all Subscribers who want a notification of this content.
        /// </summary>
        IEnumerable<Subscriber> GetSubscribers(int portalId, Frequency frequency, QueueItem queueItem);

        /// <summary>
        /// Get a list of all Subscriptions for the specified Portal ID.
        /// </summary>
        IEnumerable<Subscriber> GetAllSubscribers(int portalId);

        /// <summary>
        /// Get all subscriptions for the specified frequency that have not been published since <paramref name="publishDate"/>.
        /// </summary>
        IEnumerable<Subscriber> GetUnpublishedSubscribers(int portalId, Frequency frequency, DateTime publishDate);
                    
        /// <summary>
        /// Get the last time a Subscription task ran on Portal ID <paramref name="portalId"/>.
        /// </summary>
        DateTime? GetLastRunDate(int portalId);
    }
}