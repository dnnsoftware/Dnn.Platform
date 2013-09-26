// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Subscriptions.Controllers;
using DotNetNuke.Services.Subscriptions.Entities;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    public class SubscriptionsController : DnnApiController
	{
		#region Private Properties

		private string LocalizationFolder
		{
			get
			{
				return string.Format("~/DesktopModules/{0}/App_LocalResources/", ActiveModule.DesktopModule.FolderName);
			}
		}

		#endregion

		#region Public APIs

		/// <summary>
        /// Perform a search on Scoring Activities registered in the system.
        /// </summary>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2)</param>
        /// <param name="pageSize">Number of records to return per page</param>
        [HttpGet]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetSubscriptions(int pageIndex, int pageSize)
        {
            try
            {
                int totalRecords = 0;

                var results = Components.Subscriptions.Controllers.SubscriptionController.Instance.GetUserContentSubscriptions(PortalSettings.UserId, ActiveModule.OwnerPortalID, pageIndex, pageSize);

                if (results.Count > 0)
                {
                    totalRecords = results[0].TotalRecords;
                }
                var response = new { Success= true, Results = results, TotalResults = totalRecords };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSystemSubscription(InboxSubs post)
        {
            try
            {               
                var userPreferencesController = new UserPreferencesController();
                var userPreference = new UserPreference
                    {
                        PortalId = UserInfo.PortalID,
                        UserId = UserInfo.UserID,
                        MessagesEmailFrequency = (DotNetNuke.Services.Social.Messaging.Frequency) post.MsgFreq,
                        NotificationsEmailFrequency = (DotNetNuke.Services.Social.Messaging.Frequency) post.NotifyFreq
                    };
                userPreferencesController.SetUserPreference(userPreference);
                
                return Request.CreateResponse(HttpStatusCode.OK, userPreferencesController.GetUserPreference(UserInfo));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentSubscription(Subscriber sub)
        {
            try
            {
                SubscriptionController.Instance.DeleteSubscription(sub.SubscriberId);

                return Request.CreateResponse(HttpStatusCode.OK, "unsubscribed");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
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
					Localization.SetThreadCultures(new CultureInfo(culture), PortalSettings);
				}

				var dictionary = new Dictionary<string, string>();
				var resourcesPath = string.Format(LocalizationFolder, ActiveModule.DesktopModule.FolderName);
				var files =
					Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(resourcesPath)).Select(x => new FileInfo(x).Name).Where(f => !IsLanguageSpecific(f)).ToList();

				foreach (var kvp in files.SelectMany(f => GetLocalizationValues(resourcesPath, f, culture)).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
				{
					dictionary.Add(kvp.Key, kvp.Value);
				}

				foreach (var kvp in GetLocalizationValues(Constants.SharedResources, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
				{
					dictionary.Add(kvp.Key, kvp.Value);
				}

				return Request.CreateResponse(HttpStatusCode.OK, new { Table = dictionary });
			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);

				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
		}

        #endregion

		#region Internal classes

		public class InboxSubs
        {
            [DataMember(Name = "notifyFreq")]
            public int NotifyFreq { get; set; }

            [DataMember(Name = "msgFreq")]
            public int MsgFreq { get; set; }
            
		}

		#endregion

		#region Private Methods

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
				var document = new XmlDocument();
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

		#endregion
	}
}
