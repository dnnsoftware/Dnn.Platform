#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;

using DotNetNuke.Services.Subscriptions.Entities;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public interface ISubscriptionTypeController
    {
        /// <summary>
        /// Create a new Subscription Type
        /// </summary>
        int AddSubscriptionType(SubscriptionType subType);

        /// <summary>
        /// Retrieve all of the Subscription Types
        /// </summary>
        IList<SubscriptionType> GetTypes();

        /// <summary>
        /// Retrieve all Subscription Types registered to this Desktop Module
        /// </summary>
        IList<SubscriptionType> GetTypes(int desktopModuleId);
        
        /// <summary>
        /// Get a particular Subscription Type by ID
        /// </summary>
        SubscriptionType GetTypeById(int subscriptionTypeId);

        /// <summary>
        /// Retrieve a Subscription Type based on its name
        /// </summary>
        SubscriptionType GetTypeByName(int desktopModuleId, string subscriptionName);

        /// <summary>
        /// Delete a Subscription Type from the system. This will fail if there are any dependencies on this
        /// subscription type -- i.e., if people have subscriptions of this type, or if there are items in the
        /// queue matching this Subscription Type -- both of these cases are highly likely, and this method
        /// really only ought to be used in testing and debugging.
        /// </summary>
        bool RemoveSubscriptionType(int subscriptionTypeId);
    }
}