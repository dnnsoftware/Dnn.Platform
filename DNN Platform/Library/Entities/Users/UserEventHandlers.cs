// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users;

using System.ComponentModel.Composition;
using System.Globalization;

using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Social.Notifications;

[Export(typeof(IUserEventHandlers))]
public class UserEventHandlers : IUserEventHandlers
{
    /// <inheritdoc/>
    public void UserAuthenticated(object sender, UserEventArgs args)
    {
    }

    /// <inheritdoc/>
    public void UserCreated(object sender, UserEventArgs args)
    {
        UserRegistrationEmailNotifier.NotifyAdministrator(args.User);

        if (args.SendNotification)
        {
            UserRegistrationEmailNotifier.NotifyUser(args.User);
        }
    }

    /// <inheritdoc/>
    public void UserRemoved(object sender, UserEventArgs args)
    {
        DeleteAllNewUnauthorizedUserRegistrationNotifications(args.User.UserID);
    }

    /// <inheritdoc/>
    public void UserDeleted(object sender, UserEventArgs args)
    {
    }

    /// <inheritdoc/>
    public void UserApproved(object sender, UserEventArgs args)
    {
        if (args.SendNotification)
        {
            UserRegistrationEmailNotifier.NotifyUser(args.User, MessageType.UserRegistrationPublic);
        }

        DeleteAllNewUnauthorizedUserRegistrationNotifications(args.User.UserID);
    }

    /// <inheritdoc/>
    public void UserUpdated(object sender, UpdateUserEventArgs args)
    {
    }

    private static void DeleteAllNewUnauthorizedUserRegistrationNotifications(int userId)
    {
        var nt = NotificationsController.Instance.GetNotificationType("NewUnauthorizedUserRegistration");
        var notifications = NotificationsController.Instance.GetNotificationByContext(nt.NotificationTypeId, userId.ToString(CultureInfo.InvariantCulture));

        foreach (var notification in notifications)
        {
            NotificationsController.Instance.DeleteNotification(notification.NotificationID);
        }
    }
}
