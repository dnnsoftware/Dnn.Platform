// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Xml;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.CoreMessaging.ViewModels;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Subscriptions;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class SubscriptionsController : DnnApiController
    {
        private const string SharedResources = "~/DesktopModules/CoreMessaging/App_LocalResources/SharedResources.resx";
        private const string ViewControlResources = "~/DesktopModules/CoreMessaging/App_LocalResources/View.ascx.resx";

        private string LocalizationFolder
        {
            get
            {
                return string.Format(
                    "~/DesktopModules/{0}/App_LocalResources/",
                    DesktopModuleController.GetDesktopModuleByModuleName("DotNetNuke.Modules.CoreMessaging", this.PortalSettings.PortalId).FolderName);
            }
        }

        /// <summary>
        /// Perform a search on Scoring Activities registered in the system.
        /// </summary>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2).</param>
        /// <param name="pageSize">Number of records to return per page.</param>
        /// <param name="sortExpression">The sort expression in the form [Description|SubscriptionType] [Asc|Desc].</param>
        /// <returns>The sorted and paged list of subscriptions.</returns>
        [HttpGet]
        public HttpResponseMessage GetSubscriptions(int pageIndex, int pageSize, string sortExpression)
        {
            try
            {
                var subscriptions = from s in SubscriptionController.Instance.GetUserSubscriptions(this.UserInfo, this.PortalSettings.PortalId)
                                    select GetSubscriptionViewModel(s);

                List<SubscriptionViewModel> sortedList;
                if (string.IsNullOrEmpty(sortExpression))
                {
                    sortedList = subscriptions.ToList();
                }
                else
                {
                    var sort = sortExpression.Split(' ');
                    var desc = sort.Length == 2 && sort[1] == "desc";
                    switch (sort[0])
                    {
                        case "Description":
                            sortedList = desc
                                ? subscriptions.OrderByDescending(s => s.Description).ToList()
                                : subscriptions.OrderBy(s => s.Description).ToList();
                            break;

                        case "SubscriptionType":
                            sortedList = desc
                                ? subscriptions.OrderByDescending(s => s.SubscriptionType).ToList()
                                : subscriptions.OrderBy(s => s.SubscriptionType).ToList();
                            break;

                        default:
                            sortedList = subscriptions.ToList();
                            break;
                    }
                }

                var response = new
                {
                    Success = true,
                    Results = sortedList.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                    TotalResults = sortedList.Count(),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSystemSubscription(InboxSubscriptionViewModel post)
        {
            try
            {
                var userPreferencesController = UserPreferencesController.Instance;
                var userPreference = new UserPreference
                {
                    PortalId = this.UserInfo.PortalID,
                    UserId = this.UserInfo.UserID,
                    MessagesEmailFrequency = (Frequency)post.MsgFreq,
                    NotificationsEmailFrequency = (Frequency)post.NotifyFreq,
                };
                userPreferencesController.SetUserPreference(userPreference);

                return this.Request.CreateResponse(HttpStatusCode.OK, userPreferencesController.GetUserPreference(this.UserInfo));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentSubscription(Subscription subscription)
        {
            try
            {
                var sub = SubscriptionController.Instance.GetUserSubscriptions(this.UserInfo, this.PortalSettings.PortalId)
                                          .SingleOrDefault(s => s.SubscriptionId == subscription.SubscriptionId);
                if (sub != null)
                {
                    SubscriptionController.Instance.DeleteSubscription(sub);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, "unsubscribed");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLocalizationTable(string culture)
        {
            try
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    Localization.SetThreadCultures(new CultureInfo(culture), this.PortalSettings);
                }

                var dictionary = new Dictionary<string, string>();

                var resourcesPath = this.LocalizationFolder;
                var files =
                    Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(resourcesPath)).Select(x => new FileInfo(x).Name).Where(f => !IsLanguageSpecific(f)).ToList();

                foreach (var kvp in files.SelectMany(f => GetLocalizationValues(resourcesPath, f, culture)).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }

                foreach (var kvp in GetLocalizationValues(SharedResources, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Table = dictionary });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private static SubscriptionViewModel GetSubscriptionViewModel(Subscription subscription)
        {
            var model = new SubscriptionViewModel
            {
                SubscriptionId = subscription.SubscriptionId,
                Description = subscription.Description,
            };

            // localize the type name
            var subscriptionType = SubscriptionTypeController.Instance.GetSubscriptionType(
                t => t.SubscriptionTypeId == subscription.SubscriptionTypeId);

            if (subscriptionType != null)
            {
                var localizedName = Localization.GetString(subscriptionType.SubscriptionName, ViewControlResources);
                model.SubscriptionType = localizedName ?? subscriptionType.FriendlyName;
            }

            return model;
        }

        private static bool IsLanguageSpecific(string fileName)
        {
            var components = fileName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length > 1)
            {
                var language = components[components.Length - 2];

                if (!string.IsNullOrEmpty(language))
                {
                    try
                    {
                        CultureInfo.GetCultureInfo(language);

                        return true;
                    }
                    catch (CultureNotFoundException)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string path, string file, string culture)
        {
            return GetLocalizationValues(string.Format("{0}/{1}", path, file), culture);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            using (var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open, FileAccess.Read))
            {
                var document = new XmlDocument { XmlResolver = null };
                document.Load(stream);

                // ReSharper disable AssignNullToNotNullAttribute
                var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();

                // ReSharper restore AssignNullToNotNullAttribute
                AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

                // ReSharper disable AssignNullToNotNullAttribute
                foreach (var xmlNode in document.SelectNodes("/root/data").Cast<XmlNode>())

                // ReSharper restore AssignNullToNotNullAttribute
                {
                    var name = GetNameAttribute(xmlNode).Replace(".Text", string.Empty);

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var value = Localization.GetString(string.Format("{0}.Text", name), fullPath, culture);

                    yield return new KeyValuePair<string, string>(name, value);
                }
            }
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ApplicationException(string.Format("Resource header '{0}' != '{1}'", key, value));
                }
            }
            else
            {
                throw new ApplicationException(string.Format("Resource header '{0}' is missing", key));
            }
        }

        private static string GetNameAttribute(XmlNode node)
        {
            if (node.Attributes != null)
            {
                var attribute = node.Attributes.GetNamedItem("name");
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            return null;
        }
    }
}
