using System;
using System.Data;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Social.Subscriptions.Data
{
    public class DataService : ServiceLocator<IDataService, DataService>, IDataService
    {
        #region Private Members
        private readonly DataProvider provider;
        #endregion

        public DataService()
        {
            provider = DataProvider.Instance();
        }

        protected override Func<IDataService> GetFactory()
        {
            return () => new DataService();
        }

        #region Subscription Types
        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_AddSubscriptionType", subscriptionName, friendlyName, desktopModuleId);
        }

        public IDataReader GetSubscriptionTypes()
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionTypes");
        }

        public bool DeleteSubscriptionType(int subscriptionTypeId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscriptionType", subscriptionTypeId) == 0;
        }
        #endregion


        #region Subscriptions
        public int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_AddSubscription", 
                userId, 
                provider.GetNull(portalId),
                subscriptionTypeId, 
                objectKey, 
                description,
                provider.GetNull(moduleId),
                provider.GetNull(tabId),
                objectData);
        }

        public IDataReader GetSubscriptionsByUser(int portalId, int userId, int subscriptionTypeId)
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionsByUser", provider.GetNull(portalId), userId, provider.GetNull(subscriptionTypeId));
        }

        public IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey)
        {
            return provider.ExecuteReader("CoreMessaging_GetSubscriptionsByContent", provider.GetNull(portalId), subscriptionTypeId, objectKey);
        }

        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId)
        {
            return provider.ExecuteReader("CoreMessaging_IsSubscribed",
                provider.GetNull(portalId), 
                userId, 
                subscriptionTypeId, 
                objectKey, 
                provider.GetNull(moduleId),
                provider.GetNull(tabId));
        }

        public bool DeleteSubscription(int subscriptionId)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscription", subscriptionId) == 0;
        }

        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
            return provider.ExecuteScalar<int>("CoreMessaging_UpdateSubscriptionDescription",
                objectKey,
                portalId,
                newDescription);
        }

        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
            provider.ExecuteNonQuery("CoreMessaging_DeleteSubscriptionsByObjectKey", portalId, objectKey);
        }

        #endregion
    }
}
