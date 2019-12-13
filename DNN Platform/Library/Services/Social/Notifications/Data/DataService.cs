using System;
using System.Data;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.Social.Notifications.Data
{
    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        private readonly DataProvider _provider = DataProvider.Instance();
        private const string Prefix = "CoreMessaging_";

        #region Private Methods

        private static string GetFullyQualifiedName(string procedureName)
        {
            return Prefix + procedureName;
        }

        #endregion

        #region NotificationTypes CRUD

        public int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId, bool isTask)
        {
            return _provider.ExecuteScalar<int>(GetFullyQualifiedName("CreateNotificationType"), name, _provider.GetNull(description), _provider.GetNull(timeToLive), _provider.GetNull(desktopModuleId), createUpdateUserId, isTask);
        }

        public void DeleteNotificationType(int notificationTypeId)
        {
            _provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationType"), notificationTypeId);
        }

        public IDataReader GetNotificationType(int notificationTypeId)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationType"), notificationTypeId);
        }

        public IDataReader GetNotificationTypeByName(string name)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeByName"), name);
        }

        #endregion

        #region NotificationTypeActions CRUD

        public int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId)
        {
            return _provider.ExecuteScalar<int>(GetFullyQualifiedName("AddNotificationTypeAction"), notificationTypeId, nameResourceKey, _provider.GetNull(descriptionResourceKey), _provider.GetNull(confirmResourceKey), apiCall, createdByUserId);
        }

        public void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
            _provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationTypeAction"), notificationTypeActionId);
        }

        public IDataReader GetNotificationTypeAction(int notificationTypeActionId)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeAction"), notificationTypeActionId);
        }

        public IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActionByName"), notificationTypeId, name);
        }

        public IDataReader GetNotificationTypeActions(int notificationTypeId)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActions"), notificationTypeId);
        }

        #endregion

        #region Notifications Public Methods

        public int SendNotification(Notification notification, int portalId)
        {
            var createdByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            return _provider.ExecuteScalar<int>(GetFullyQualifiedName("SendNotification"),
                                                notification.NotificationTypeID,
                                                portalId,
                                                notification.To,
                                                notification.From,
                                                notification.Subject,
                                                notification.Body,
                                                notification.SenderUserID,
                                                createdByUserId,
                                                _provider.GetNull(notification.ExpirationDate),
                                                notification.IncludeDismissAction,
                                                notification.Context);
        }

        public void DeleteNotification(int notificationId)
        {
            _provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotification"), notificationId);
        }

        public int DeleteUserNotifications(int userId, int portalId)
        {
            return userId <= 0 ? 0 : _provider.ExecuteScalar<int>(GetFullyQualifiedName("DeleteUserNotifications"), userId, portalId);
        }

        public int CountNotifications(int userId, int portalId)
        {
            return userId <= 0 ? 0 : _provider.ExecuteScalar<int>(GetFullyQualifiedName("CountNotifications"), userId, portalId);
        }

        public IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotifications"), userId, portalId, afterNotificationId, numberOfRecords);
        }

        public IDataReader GetNotification(int notificationId)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotification"), notificationId);
        }

        public IDataReader GetNotificationByContext(int notificationTypeId, string context)
        {
            return _provider.ExecuteReader(GetFullyQualifiedName("GetNotificationByContext"), notificationTypeId, context);
        }

        #endregion

		#region Toast

		public bool IsToastPending(int notificationId)
		{
			return _provider.ExecuteScalar<bool>(GetFullyQualifiedName("IsToastPending"),
												notificationId);
		}

		/// <summary>
		/// Mark a Toast ready for sending
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		public void MarkReadyForToast(int notificationId, int userId)
		{
			_provider.ExecuteNonQuery(GetFullyQualifiedName("MarkReadyForToast"), notificationId, userId);
		}

		/// <summary>
		/// Mark Toast being already sent
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		public void MarkToastSent(int notificationId, int userId)
		{
            _provider.ExecuteNonQuery(GetFullyQualifiedName("MarkToastSent"), notificationId, userId);
		}

		public IDataReader GetToasts(int userId, int portalId)
		{
			return _provider.ExecuteReader(GetFullyQualifiedName("GetToasts"), userId, portalId);
		}

		#endregion

    }
}
