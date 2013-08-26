#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using System.Text;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Subscriptions.Components.Entities;
using DotNetNuke.Subscriptions.Components.Entities.Templates;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class TokenizeControllerImpl : BaseCustomTokenReplace, ITokenizeController
    {
        #region Constructors

        public TokenizeControllerImpl()
            : this(new Actions(), TemplateSettingsReader.Instance.GetSettings())
        {
            _contentController = new ContentController() as IContentController;
        }

        public TokenizeControllerImpl(Actions actions, TemplateSettings settings)
        {
            _actions = actions;
            _templateSettings = settings;
            CurrentAccessLevel = Scope.DefaultSettings;
        }

        #endregion

        #region Private members

        private readonly Actions _actions;

        private readonly TemplateSettings _templateSettings;

        private readonly IContentController _contentController;

        #endregion

        #region Implementation of ITokenizeController

        public FormattedNotification Tokenize(UserInfo author, InstantNotification instantNotification)
        {
            PropertySource.Clear();

            PropertySource["author"] = new AuthorContent(author);
            PropertySource["actions"] = new ActionsContent(_actions);
            PropertySource["item"] = new QueueItemContent(instantNotification.QueueItem);

            return new FormattedNotification(author, instantNotification.Subscribers)
                {
                    Subject = ReplaceTokens(_templateSettings.InstantSubject),
                    Body = ReplaceTokens(ConstructMessage(instantNotification))
                };
        }

        public FormattedNotification Tokenize(DigestNotification digestNotification)
        {
            PropertySource.Clear();
            PropertySource["actions"] = new ActionsContent(_actions);
            PropertySource["digest"] = new DigestSummaryContent(digestNotification);
            PropertySource["subscriber"] = new SubscriberContent(digestNotification.Subscriber);

            var content = new StringBuilder();

            content.AppendLine(ReplaceTokens(_templateSettings.DigestHeader));

            foreach (var item in digestNotification.QueueItems)
            {
                PropertySource["item"] = new QueueItemContent(item);

                var author = GetAuthor(item);
                if (author != null)
                {
                    PropertySource["author"] = new AuthorContent(author);
                }

                content.AppendLine(ReplaceTokens(_templateSettings.ItemHeader));
                content.AppendLine(ReplaceTokens(_templateSettings.Item));
                content.AppendLine(ReplaceTokens(_templateSettings.ItemFooter));
            }

            content.AppendLine(ReplaceTokens(_templateSettings.DigestFooter));

            return new FormattedNotification(null, new List<Subscriber>(new[] {digestNotification.Subscriber}))
                {
                    Subject = ReplaceTokens(_templateSettings.DigestSubject),
                    Body = content.ToString()
                };
        }

        #endregion

        #region Private methods

        private string ConstructMessage(InstantNotification instantNotification)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_templateSettings.InstantHeader);
            sb.AppendLine(_templateSettings.ItemHeader);
            sb.AppendLine(_templateSettings.Item);
            sb.AppendLine(_templateSettings.ItemFooter);
            sb.AppendLine(_templateSettings.InstantFooter);

            return sb.ToString();
        }

        private UserInfo GetAuthor(QueueItem queueItem)
        {
            var contentItem = _contentController.GetContentItem(queueItem.ContentItemId);
            if (contentItem != null)
            {
                return contentItem.CreatedByUser(queueItem.PortalId);
            }

            return null;
        }

        #endregion
    }
}