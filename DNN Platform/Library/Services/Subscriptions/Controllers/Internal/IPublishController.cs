#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public interface IPublishController
    {
        /// <summary>
        /// Publish an instant notification to a particular Subscriber
        /// </summary>
        void Publish(InstantNotification instantNotification);

        /// <summary>
        /// Publish a digest notification to a particular Subscriber
        /// </summary>
        void Publish(DigestNotification digestNotification);
    }
}