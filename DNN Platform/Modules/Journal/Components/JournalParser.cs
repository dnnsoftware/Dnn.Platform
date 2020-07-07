// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Journal.Internal;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class JournalParser
    {
        private const string ResxPath = "~/DesktopModules/Journal/App_LocalResources/SharedResources.resx";
        private static readonly Regex CdataRegex = new Regex(@"\<\!\[CDATA\[(?<text>[^\]]*)\]\]\>", RegexOptions.Compiled);
        private static readonly Regex TemplateRegex = new Regex("{CanComment}(.*?){/CanComment}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BaseUrlRegex = new Regex("\\[BaseUrl\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string url = string.Empty;
        private bool isAdmin;
        private bool isUnverifiedUser;

        public JournalParser(PortalSettings portalSettings, int moduleId, int profileId, int socialGroupId, UserInfo userInfo)
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.PortalSettings = portalSettings;
            this.ModuleId = moduleId;
            this.ProfileId = profileId;
            this.SocialGroupId = socialGroupId;
            this.CurrentUser = userInfo;
            this.url = this.PortalSettings.DefaultPortalAlias;
            this.OwnerPortalId = portalSettings.PortalId;
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(moduleId, this.PortalSettings.ActiveTab.TabID, false);
            if (moduleInfo.OwnerPortalID != portalSettings.PortalId)
            {
                this.OwnerPortalId = moduleInfo.OwnerPortalID;
            }

            if (string.IsNullOrEmpty(this.url))
            {
                this.url = HttpContext.Current.Request.Url.Host;
            }

            this.url = string.Format(
                "{0}://{1}{2}",
                UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request) ? "https" : "http",
                this.url,
                !HttpContext.Current.Request.Url.IsDefaultPort && !this.url.Contains(":") ? ":" + HttpContext.Current.Request.Url.Port : string.Empty);
        }

        public int JournalId { get; set; }

        protected INavigationManager NavigationManager { get; }

        private PortalSettings PortalSettings { get; set; }

        private int ProfileId { get; set; }

        private int SocialGroupId { get; set; }

        private int ModuleId { get; set; }

        private UserInfo CurrentUser { get; set; }

        private int OwnerPortalId { get; set; }

        public string GetList(int currentIndex, int rows)
        {
            if (this.CurrentUser.UserID > 0)
            {
                this.isAdmin = this.CurrentUser.IsInRole(this.PortalSettings.AdministratorRoleName);
            }

            this.isUnverifiedUser = !this.CurrentUser.IsSuperUser && this.CurrentUser.IsInRole("Unverified Users");

            var journalControllerInternal = InternalJournalController.Instance;
            var sb = new StringBuilder();

            string statusTemplate = Localization.GetString("journal_status", ResxPath);
            string linkTemplate = Localization.GetString("journal_link", ResxPath);
            string photoTemplate = Localization.GetString("journal_photo", ResxPath);
            string fileTemplate = Localization.GetString("journal_file", ResxPath);

            statusTemplate = BaseUrlRegex.Replace(statusTemplate, this.url);
            linkTemplate = BaseUrlRegex.Replace(linkTemplate, this.url);
            photoTemplate = BaseUrlRegex.Replace(photoTemplate, this.url);
            fileTemplate = BaseUrlRegex.Replace(fileTemplate, this.url);

            string comment = Localization.GetString("comment", ResxPath);

            IList<JournalItem> journalList;
            if (this.JournalId > 0)
            {
                var journal = JournalController.Instance.GetJournalItem(this.PortalSettings.PortalId, this.CurrentUser.UserID,
                                                                        this.JournalId, false, false, true);
                journalList = new List<JournalItem>();
                if (journal != null)
                {
                    journalList.Add(journal);
                }
            }
            else if (this.ProfileId > 0)
            {
                journalList = journalControllerInternal.GetJournalItemsByProfile(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, this.ProfileId, currentIndex, rows);
            }
            else if (this.SocialGroupId > 0)
            {
                journalList = journalControllerInternal.GetJournalItemsByGroup(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, this.SocialGroupId, currentIndex, rows);
            }
            else
            {
                journalList = journalControllerInternal.GetJournalItems(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, currentIndex, rows);
            }

            var journalIds = journalList.Select(ji => ji.JournalId).ToList();
            IList<CommentInfo> comments = JournalController.Instance.GetCommentsByJournalIds(journalIds);

            foreach (JournalItem ji in journalList)
            {
                string replacement = this.GetStringReplacement(ji);

                string rowTemplate;
                if (ji.JournalType == "status")
                {
                    rowTemplate = statusTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "link")
                {
                    rowTemplate = linkTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "photo")
                {
                    rowTemplate = photoTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "file")
                {
                    rowTemplate = fileTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else
                {
                    rowTemplate = this.GetJournalTemplate(ji.JournalType, ji);
                }

                var ctl = new JournalControl();

                bool isLiked = false;
                ctl.LikeList = this.GetLikeListHTML(ji, ref isLiked);
                ctl.LikeLink = string.Empty;
                ctl.CommentLink = string.Empty;

                ctl.AuthorNameLink = "<a href=\"" + this.NavigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId=" + ji.JournalAuthor.Id }) + "\">" + ji.JournalAuthor.Name + "</a>";
                if (this.CurrentUser.UserID > 0 && !this.isUnverifiedUser)
                {
                    if (!ji.CommentsDisabled)
                    {
                        ctl.CommentLink = "<a href=\"#\" id=\"cmtbtn-" + ji.JournalId + "\">" + comment + "</a>";
                    }

                    if (isLiked)
                    {
                        ctl.LikeLink = "<a href=\"#\" id=\"like-" + ji.JournalId + "\">{resx:unlike}</a>";
                    }
                    else
                    {
                        ctl.LikeLink = "<a href=\"#\" id=\"like-" + ji.JournalId + "\">{resx:like}</a>";
                    }
                }

                ctl.CommentArea = this.GetCommentAreaHTML(ji, comments);
                ji.TimeFrame = DateUtils.CalculateDateForDisplay(ji.DateCreated);
                ji.DateCreated = this.CurrentUser.LocalTime(ji.DateCreated);

                if (ji.Summary != null)
                {
                    ji.Summary = ji.Summary.Replace("\n", "<br />");
                }

                if (ji.Body != null)
                {
                    ji.Body = ji.Body.Replace(Environment.NewLine, "<br />");
                }

                var tokenReplace = new JournalItemTokenReplace(ji, ctl);
                string tmp = tokenReplace.ReplaceJournalItemTokens(rowTemplate);
                tmp = tmp.Replace("<br>", "<br />");
                sb.Append("<div class=\"journalrow\" id=\"jid-" + ji.JournalId + "\">");
                if (this.isAdmin || this.CurrentUser.UserID == ji.UserId || (this.ProfileId > Null.NullInteger && this.CurrentUser.UserID == this.ProfileId))
                {
                    sb.Append("<div class=\"minidel\" onclick=\"journalDelete(this);\"></div>");
                }

                sb.Append(tmp);
                sb.Append("</div>");
            }

            return Utilities.LocalizeControl(sb.ToString());
        }

        internal string GetJournalTemplate(string journalType, JournalItem ji)
        {
            string template = Localization.GetString("journal_" + journalType, ResxPath);
            if (string.IsNullOrEmpty(template))
            {
                template = Localization.GetString("journal_generic", ResxPath);
            }

            template = BaseUrlRegex.Replace(template, this.url);
            template = template.Replace("[journalitem:action]", Localization.GetString(journalType + ".Action", ResxPath));

            var replacement = this.GetStringReplacement(ji);
            return TemplateRegex.Replace(template, replacement);
        }

        internal string GetLikeListHTML(JournalItem ji, ref bool isLiked)
        {
            var sb = new StringBuilder();
            isLiked = false;
            if (ji.JournalXML == null)
            {
                return string.Empty;
            }

            XmlNodeList xLikes = ji.JournalXML.DocumentElement.SelectNodes("//likes/u");
            if (xLikes == null)
            {
                return string.Empty;
            }

            foreach (XmlNode xLike in xLikes)
            {
                if (Convert.ToInt32(xLike.Attributes["uid"].Value) == this.CurrentUser.UserID)
                {
                    ji.CurrentUserLikes = true;
                    isLiked = true;
                    break;
                }
            }

            int xc = 0;
            sb.Append("<div class=\"likes\">");
            if (xLikes.Count == 1 && ji.CurrentUserLikes)
            {
                sb.Append("{resx:youlikethis}");
            }
            else if (xLikes.Count > 1)
            {
                if (ji.CurrentUserLikes)
                {
                    sb.Append("{resx:you}");
                    xc += 1;
                }

                foreach (XmlNode l in xLikes)
                {
                    int userId = Convert.ToInt32(l.Attributes["uid"].Value);
                    string name = l.Attributes["un"].Value;
                    if (userId != this.CurrentUser.UserID)
                    {
                        if (xc < xLikes.Count - 1 && xc > 0 && xc < 3)
                        {
                            sb.Append(", ");
                        }
                        else if (xc > 0 & xc < xLikes.Count & xc < 3)
                        {
                            sb.Append(" {resx:and} ");
                        }
                        else if (xc >= 3)
                        {
                            int diff = xLikes.Count - xc;
                            sb.Append(" {resx:and} " + (xLikes.Count - xc).ToString(CultureInfo.InvariantCulture));
                            if (diff > 1)
                            {
                                sb.Append(" {resx:others}");
                            }
                            else
                            {
                                sb.Append(" {resx:other}");
                            }

                            break;
                        }

                        sb.AppendFormat("<span id=\"user-{0}\" class=\"juser\">{1}</span>", userId, name);
                        xc += 1;
                    }
                }

                if (xc == 1)
                {
                    sb.Append(" {resx:likesthis}");
                }
                else if (xc > 1)
                {
                    sb.Append(" {resx:likethis}");
                }
            }
            else
            {
                foreach (XmlNode l in xLikes)
                {
                    int userId = Convert.ToInt32(l.Attributes["uid"].Value);
                    string name = l.Attributes["un"].Value;
                    sb.AppendFormat("<span id=\"user-{0}\" class=\"juser\">{1}</span>", userId, name);
                    xc += 1;
                    if (xc == xLikes.Count - 1)
                    {
                        sb.Append(" {resx:and} ");
                    }
                    else if (xc < xLikes.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                if (xc == 1)
                {
                    sb.Append(" {resx:likesthis}");
                }
                else if (xc > 1)
                {
                    sb.Append(" {resx:likethis}");
                }
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        internal string GetCommentAreaHTML(JournalItem journal, IList<CommentInfo> comments)
        {
            if (journal.CommentsHidden)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("<ul class=\"jcmt\" id=\"jcmt-{0}\">", journal.JournalId);
            foreach (CommentInfo ci in comments)
            {
                if (ci.JournalId == journal.JournalId)
                {
                    sb.Append(this.GetCommentRow(journal, ci));
                }
            }

            if (this.CurrentUser.UserID > 0 && !journal.CommentsDisabled)
            {
                sb.AppendFormat("<li id=\"jcmt-{0}-txtrow\" class=\"cmteditarea\">", journal.JournalId);
                sb.AppendFormat("<textarea id=\"jcmt-{0}-txt\" class=\"cmteditor\"></textarea>", journal.JournalId);
                sb.Append("<div class=\"editorPlaceholder\">{resx:leavecomment}</div></li>");
                sb.Append("<li class=\"cmtbtn\">");
                sb.Append("<a href=\"#\">{resx:comment}</a></li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        internal string GetCommentRow(JournalItem journal, CommentInfo comment)
        {
            var sb = new StringBuilder();
            string pic = UserController.Instance.GetUserProfilePictureUrl(comment.UserId, 32, 32);
            sb.AppendFormat("<li id=\"cmt-{0}\">", comment.CommentId);
            if (comment.UserId == this.CurrentUser.UserID || journal.UserId == this.CurrentUser.UserID || this.isAdmin)
            {
                sb.Append("<div class=\"miniclose\"></div>");
            }

            sb.AppendFormat("<img src=\"{0}\" />", pic);
            sb.Append("<p>");
            string userUrl = this.NavigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId=" + comment.UserId });
            sb.AppendFormat("<a href=\"{1}\">{0}</a>", comment.DisplayName, userUrl);

            if (comment.CommentXML != null && comment.CommentXML.SelectSingleNode("/root/comment") != null)
            {
                string text;
                if (CdataRegex.IsMatch(comment.CommentXML.SelectSingleNode("/root/comment").InnerText))
                {
                    var match = CdataRegex.Match(comment.CommentXML.SelectSingleNode("/root/comment").InnerText);
                    text = match.Groups["text"].Value;
                }
                else
                {
                    text = comment.CommentXML.SelectSingleNode("/root/comment").InnerText;
                }

                sb.Append(text.Replace("\n", "<br />"));
            }
            else
            {
                sb.Append(comment.Comment.Replace("\n", "<br />"));
            }

            var timeFrame = DateUtils.CalculateDateForDisplay(comment.DateCreated);
            comment.DateCreated = this.CurrentUser.LocalTime(comment.DateCreated);
            sb.AppendFormat("<abbr title=\"{0}\">{1}</abbr>", comment.DateCreated, timeFrame);

            sb.Append("</p>");
            sb.Append("</li>");
            return sb.ToString();
        }

        private string GetStringReplacement(JournalItem journalItem)
        {
            string replacement = string.Empty;
            if (this.CurrentUser.UserID > 0 && this.SocialGroupId <= 0 && !this.isUnverifiedUser)
            {
                replacement = "$1";
            }

            if (this.CurrentUser.UserID > 0 && journalItem.SocialGroupId > 0 && !this.isUnverifiedUser)
            {
                replacement = this.CurrentUser.IsInRole(journalItem.JournalOwner.Name) ? "$1" : string.Empty;
            }

            return replacement;
        }
    }
}
