// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Subscriptions.Components.Controllers;
using DotNetNuke.Subscriptions.Components.Entities;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Services
{
    public class SubscriptionsController : DnnApiController
    {
        #region Public APIs

        /// <summary>
        /// Perform a search on Scoring Activities registered in the system.
        /// </summary>
        /// <param name="searchText">Search filter text (if any)</param>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2)</param>
        /// <param name="pageSize">Number of records to return per page</param>
        /// <param name="sortColumn">Column to sort on</param>
        /// <param name="sortAscending">Sort ascending or descending</param>
        [HttpGet]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetSubscriptions(int pageIndex, int pageSize)
        {
            try
            {
                int totalRecords = 0;

                var results = Components.Controllers.SubscriptionController.Instance.GetUserContentSubscriptions(PortalSettings.UserId, ActiveModule.OwnerPortalID, pageIndex, pageSize);

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
                var sub = new Subscriber
                    {
                        SubscriptionTypeId = post.NotiySubscriptionTypeId,
                        UserId = UserInfo.UserID,
                        PortalId = UserInfo.PortalID,
                        Frequency = (Frequency) post.NotifyFreq,
                        SubscriberId = post.NotifySubscriberId,
                        GroupId = Null.NullInteger,
                        ObjectKey = Null.NullString,
                        ModuleId = Null.NullInteger,
                        ContentItemId = Null.NullInteger
                    };
                SubscriptionController.Instance.Subscribe(sub);

                sub = new Subscriber
                    {
                        SubscriptionTypeId = post.MsgSubscriptionTypeId,
                        UserId = UserInfo.UserID,
                        PortalId = UserInfo.PortalID,
                        Frequency = (Frequency) post.MsgFreq,
                        SubscriberId = post.MsgSubscriberId,
                        GroupId = Null.NullInteger,
                        ObjectKey = Null.NullString,
                        ModuleId = Null.NullInteger,
                        ContentItemId = Null.NullInteger
                    };
                SubscriptionController.Instance.Subscribe(sub);

                return Request.CreateResponse(HttpStatusCode.OK, Components.Controllers.SubscriptionController.Instance.GetUserInboxSubscriptions(PortalSettings.UserId, ActiveModule.OwnerPortalID));
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

        #endregion

        public class InboxSubs
        {
            [DataMember(Name = "notifySubscriberId")]
            public int NotifySubscriberId { get; set; }

            [DataMember(Name = "msgSubscriberId")]
            public int MsgSubscriberId { get; set; }

            [DataMember(Name = "notifyFreq")]
            public int NotifyFreq { get; set; }

            [DataMember(Name = "msgFreq")]
            public int MsgFreq { get; set; }

            [DataMember(Name = "NotifySubscriptionTypeId")]
            public int NotiySubscriptionTypeId { get; set; }

            [DataMember(Name = "MsgSubscriptionTypeId")]
            public int MsgSubscriptionTypeId { get; set; }
        }

    }
}
