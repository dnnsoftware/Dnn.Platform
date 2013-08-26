#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class TemplateControllerImpl : ITemplateController
    {
        #region Constructors

        public TemplateControllerImpl()
        {
            _templateSettings = TemplateSettingsReader.Instance.GetSettings();

            _contentController = new ContentController();
        }
        
        #endregion

        #region Private members

        private readonly TemplateSettings _templateSettings;

        private readonly IContentController _contentController;

        #endregion

        #region Implementation of ITemplateController

        public FormattedNotification Format(InstantNotification instantNotification)
        {
            var author = GetContentAuthor(
                instantNotification.QueueItem.PortalId,
                instantNotification.QueueItem.ContentItemId);

            return TokenizeController.Instance.Tokenize(author, instantNotification);
        }

        public FormattedNotification Format(DigestNotification digestNotification)
        {
            return TokenizeController.Instance.Tokenize(digestNotification);
        }

        #endregion

        #region Private methods

        private UserInfo GetContentAuthor(int portalId, int contentItemId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);
            if (contentItem != null)
            {
                return contentItem.LastModifiedOnDate > contentItem.CreatedOnDate
                           ? contentItem.LastModifiedByUser(portalId)
                           : contentItem.CreatedByUser(portalId);
            }

            return null;
        }

        private static UserInfo GetAdminUser(int portalId)
        {
            var portalSettings = new PortalSettings(portalId);

            return UserController.GetUserById(portalId, portalSettings.AdministratorId);
        }

        #endregion
    }
}