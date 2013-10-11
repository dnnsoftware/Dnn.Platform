using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Journal;
using DotNetNuke.Entities.Users;
using System.Text;
using DotNetNuke.Entities.Portals;
using System.Text.RegularExpressions;

using DotNetNuke.Services.Journal.Internal;
using DotNetNuke.Services.Localization;
using System.Xml;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.Journal.Components 
{
	public class JournalParser 
    {
	    PortalSettings PortalSettings { get; set; }
		int ProfileId { get; set; }
		int SocialGroupId { get; set; }
        int ModuleId { get; set; }
		UserInfo CurrentUser { get; set; }
        int OwnerPortalId { get; set; }
        private readonly string url = "";
        private bool isAdmin;
	    private bool isUnverifiedUser;
	    private const string ResxPath = "~/DesktopModules/Journal/App_LocalResources/SharedResources.resx";

	    public JournalParser(PortalSettings portalSettings, int moduleId, int profileId, int socialGroupId, UserInfo userInfo) 
        {
			PortalSettings = portalSettings;
            ModuleId = moduleId;
			ProfileId = profileId;
			SocialGroupId = socialGroupId;
			CurrentUser = userInfo;
            url = PortalSettings.DefaultPortalAlias;
            OwnerPortalId = portalSettings.PortalId;
            var moduleController = new ModuleController();
            ModuleInfo moduleInfo = moduleController.GetModule(moduleId);
            if (moduleInfo.OwnerPortalID != portalSettings.PortalId)
            {
                OwnerPortalId = moduleInfo.OwnerPortalID;
            }

            if (string.IsNullOrEmpty(url)) {
                url = HttpContext.Current.Request.Url.Host;
            }
		    url = string.Format("{0}://{1}{2}",
                                UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request) ? "https" : "http",
		                        url,
								!HttpContext.Current.Request.Url.IsDefaultPort && !url.Contains(":") ? ":" + HttpContext.Current.Request.Url.Port : string.Empty);
		}

		public string GetList(int currentIndex, int rows) 
        {
            if (CurrentUser.UserID > 0) {
                isAdmin = CurrentUser.IsInRole(PortalSettings.AdministratorRoleName);
            }
		    isUnverifiedUser = !CurrentUser.IsSuperUser && CurrentUser.IsInRole("Unverified Users");

			var journalControllerInternal = InternalJournalController.Instance;
			var sb = new StringBuilder();		

            string statusTemplate = Localization.GetString("journal_status", ResxPath);
            string linkTemplate = Localization.GetString("journal_link", ResxPath);
            string photoTemplate = Localization.GetString("journal_photo", ResxPath);
            string fileTemplate = Localization.GetString("journal_file", ResxPath);

		    statusTemplate = Regex.Replace(statusTemplate, "\\[BaseUrl\\]", url, RegexOptions.IgnoreCase);
            linkTemplate = Regex.Replace(linkTemplate, "\\[BaseUrl\\]", url, RegexOptions.IgnoreCase);
            photoTemplate = Regex.Replace(photoTemplate, "\\[BaseUrl\\]", url, RegexOptions.IgnoreCase);
            fileTemplate = Regex.Replace(fileTemplate, "\\[BaseUrl\\]", url, RegexOptions.IgnoreCase);

            string comment = Localization.GetString("comment", ResxPath);
            
            IList<JournalItem> journalList;
            if (ProfileId > 0) {
                journalList = journalControllerInternal.GetJournalItemsByProfile(OwnerPortalId, ModuleId, CurrentUser.UserID, ProfileId, currentIndex, rows);
            } else if (SocialGroupId > 0) {
                journalList = journalControllerInternal.GetJournalItemsByGroup(OwnerPortalId, ModuleId, CurrentUser.UserID, SocialGroupId, currentIndex, rows);
            } else {
                journalList = journalControllerInternal.GetJournalItems(OwnerPortalId, ModuleId, CurrentUser.UserID, currentIndex, rows);
            }

            var journalIds = journalList.Select(ji => ji.JournalId).ToList();
            IList<CommentInfo> comments = JournalController.Instance.GetCommentsByJournalIds(journalIds);

			foreach (JournalItem ji in journalList) {
                const string pattern = "{CanComment}(.*?){/CanComment}";
			    string replacement = GetStringReplacement(ji);

			    string rowTemplate;
			    if (ji.JournalType == "status") {
                    rowTemplate = statusTemplate;
                    rowTemplate = Regex.Replace(rowTemplate, pattern, replacement, RegexOptions.IgnoreCase);
                } else if (ji.JournalType == "link") {
                    rowTemplate = linkTemplate;
                    rowTemplate = Regex.Replace(rowTemplate, pattern, replacement, RegexOptions.IgnoreCase);
                } else if (ji.JournalType == "photo") {
                    rowTemplate = photoTemplate;
                    rowTemplate = Regex.Replace(rowTemplate, pattern, replacement, RegexOptions.IgnoreCase);
                } else if (ji.JournalType == "file") {
                    rowTemplate = fileTemplate;
                    rowTemplate = Regex.Replace(rowTemplate, pattern, replacement, RegexOptions.IgnoreCase);
                } else {
                    rowTemplate = GetJournalTemplate(ji.JournalType, ji);
                }
                
				var ctl = new JournalControl();
				
                bool isLiked = false;
                ctl.LikeList = GetLikeListHTML(ji, ref isLiked);
                ctl.LikeLink = String.Empty;
                ctl.CommentLink = String.Empty;
                
                ctl.AuthorNameLink = "<a href=\"" + Globals.NavigateURL(PortalSettings.UserTabId, string.Empty, new[] {"userId=" + ji.JournalAuthor.Id}) + "\">" + ji.JournalAuthor.Name + "</a>";
                if (CurrentUser.UserID > 0 &&  !isUnverifiedUser) 
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
                
                ctl.CommentArea = GetCommentAreaHTML(ji, comments);
				ji.TimeFrame = DateUtils.CalculateDateForDisplay(ji.DateCreated);
                ji.DateCreated = CurrentUser.LocalTime(ji.DateCreated);
 
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
                if (isAdmin || CurrentUser.UserID == ji.UserId || (ProfileId > Null.NullInteger && CurrentUser.UserID == ProfileId)) {
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
            if (String.IsNullOrEmpty(template))
            {
                template = Localization.GetString("journal_generic", ResxPath);
            }

            template = Regex.Replace(template, "\\[BaseUrl\\]", url, RegexOptions.IgnoreCase);
            template = template.Replace("[journalitem:action]", Localization.GetString(journalType + ".Action", ResxPath));

            const string pattern = "{CanComment}(.*?){/CanComment}";
            string replacement = GetStringReplacement(ji);

            template = Regex.Replace(template, pattern, replacement, RegexOptions.IgnoreCase);

            return template;
        }

		internal string GetLikeListHTML(JournalItem ji, ref bool isLiked) 
        {
			var sb = new StringBuilder();
            isLiked = false;
			if (ji.JournalXML == null) {
				return string.Empty;
			}
			XmlNodeList xLikes = ji.JournalXML.DocumentElement.SelectNodes("//likes/u");
			if (xLikes == null){
				return string.Empty;
			}
			 foreach(XmlNode xLike in xLikes) {
				if (Convert.ToInt32(xLike.Attributes["uid"].Value) == CurrentUser.UserID){
					ji.CurrentUserLikes = true;
                    isLiked = true;
					break;
				}

			}
			 int xc = 0;
			sb.Append("<div class=\"likes\">");
			if (xLikes.Count == 1 && ji.CurrentUserLikes) {
				sb.Append("{resx:youlikethis}");
			} else if (xLikes.Count > 1) {
				if (ji.CurrentUserLikes) {
					sb.Append("{resx:you}");
					xc += 1;
				}
				foreach (XmlNode l in xLikes) {
					int userId = Convert.ToInt32(l.Attributes["uid"].Value);
					string name = l.Attributes["un"].Value;
					if (userId != CurrentUser.UserID) {
						if (xc < xLikes.Count - 1 && xc > 0 && xc < 3) {
							sb.Append(", ");
						} else if (xc > 0 & xc < xLikes.Count & xc < 3) {
							sb.Append(" {resx:and} ");
						} else if (xc >= 3) {
							int diff = (xLikes.Count - xc);
							sb.Append(" {resx:and} " + (xLikes.Count - xc).ToString(CultureInfo.InvariantCulture));
							if (diff > 1) {
								sb.Append(" {resx:others}");
							} else {
								sb.Append(" {resx:other}");
							}
							break; // TODO: might not be correct. Was : Exit For
						}
						sb.AppendFormat("<span id=\"user-{0}\" class=\"juser\">{1}</span>", userId, name);
						xc += 1;
					}
				}
				if (xc == 1) {
					sb.Append(" {resx:likesthis}");
				} else if (xc>1) {
					sb.Append(" {resx:likethis}");
				}

		} else {
			foreach (XmlNode l in xLikes) {
				int userId = Convert.ToInt32(l.Attributes["uid"].Value);
				string name = l.Attributes["un"].Value;
				sb.AppendFormat("<span id=\"user-{0}\" class=\"juser\">{1}</span>", userId, name);
				xc += 1;
				if (xc == xLikes.Count - 1) {
					sb.Append(" {resx:and} ");
				} else if (xc < xLikes.Count - 1) {
					sb.Append(", ");
				}
			}
			if (xc == 1) {
				sb.Append(" {resx:likesthis}");
			} else if (xc>1) {
				sb.Append(" {resx:likethis}");
			}
		}

		   
			sb.Append("</div>");
			return sb.ToString();
		}

        internal string GetCommentAreaHTML(JournalItem journal, IList<CommentInfo> comments) {
            if (journal.CommentsHidden)
            {
                return string.Empty;
            }
            var sb = new StringBuilder();
			sb.AppendFormat("<ul class=\"jcmt\" id=\"jcmt-{0}\">", journal.JournalId);
            foreach(CommentInfo ci in comments) {
				if (ci.JournalId == journal.JournalId)
				{
					sb.Append(GetCommentRow(journal, ci));
                }
            }
			if (CurrentUser.UserID > 0 && !journal.CommentsDisabled)
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

        internal string GetCommentRow(JournalItem journal, CommentInfo comment) {
            var sb = new StringBuilder();
            string pic = string.Format(Globals.UserProfilePicFormattedUrl(), comment.UserId, 32, 32);
            sb.AppendFormat("<li id=\"cmt-{0}\">", comment.CommentId);
            if (comment.UserId == CurrentUser.UserID || journal.UserId == CurrentUser.UserID || isAdmin) {
                sb.Append("<div class=\"miniclose\"></div>");
            }
            sb.AppendFormat("<img src=\"{0}\" />", pic);
            sb.Append("<p>");
            string userUrl = Globals.NavigateURL(PortalSettings.UserTabId, string.Empty, new[] { "userId=" + comment.UserId });
            sb.AppendFormat("<a href=\"{1}\">{0}</a>", comment.DisplayName, userUrl);
            
            if (comment.CommentXML != null && comment.CommentXML.SelectSingleNode("/root/comment") != null)
            {
                string text;
                var regex = new Regex(@"\<\!\[CDATA\[(?<text>[^\]]*)\]\]\>");
                if (regex.IsMatch(comment.CommentXML.SelectSingleNode("/root/comment").InnerText))
                {
                    var match = regex.Match(comment.CommentXML.SelectSingleNode("/root/comment").InnerText);
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
            comment.DateCreated = CurrentUser.LocalTime(comment.DateCreated);
            sb.AppendFormat("<abbr title=\"{0}\">{1}</abbr>", comment.DateCreated, timeFrame);
  
            sb.Append("</p>");
            sb.Append("</li>");
            return sb.ToString();
        }

        #region Private Methods
        private string GetStringReplacement(JournalItem journalItem)
        {
            string replacement = string.Empty;
            if (CurrentUser.UserID > 0 && SocialGroupId <= 0 && !isUnverifiedUser)
            {
                replacement = "$1";
            }
            if (CurrentUser.UserID > 0 && journalItem.SocialGroupId > 0 && !isUnverifiedUser)
            {
                replacement = CurrentUser.IsInRole(journalItem.JournalOwner.Name) ? "$1" : string.Empty;
            }
            return replacement;
        }
        #endregion
    }
}