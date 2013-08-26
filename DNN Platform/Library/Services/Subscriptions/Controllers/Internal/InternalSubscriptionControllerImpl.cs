#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Subscriptions.Components.Entities;
using DotNetNuke.Subscriptions.Providers.Data;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class InternalSubscriptionControllerImpl : IInternalSubscriptionController
    {
        #region Private members

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public InternalSubscriptionControllerImpl()
            : this(new DataService())
        {
        }

        public InternalSubscriptionControllerImpl(IDataService dataService)
        {
            _dataService = dataService;
        }

        #endregion

        #region Implementation of IInternalSubscriptionController

        public IEnumerable<Subscriber> GetSubscribers(int portalId, Frequency frequency, QueueItem queueItem)
        {
            IDataReader reader = null;

            try
            {
                reader = _dataService.GetSubscribers(portalId, (int) frequency, queueItem);

                while (reader.Read())
                {
                    yield return CBO.FillObject<Subscriber>(reader, false);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public IEnumerable<Subscriber> GetUnpublishedSubscribers(int portalId, Frequency frequency, DateTime publishDate)
        {
            IDataReader reader = null;

            try
            {
                reader = _dataService.GetUnpublishedSubscribers(portalId, (int) frequency, publishDate.ToUniversalTime());

                while (reader.Read())
                {
                    yield return CBO.FillObject<Subscriber>(reader, false);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public IEnumerable<Subscriber> GetAllSubscribers(int portalId)
        {
            IDataReader reader = null;

            try
            {
                reader = _dataService.GetAllSubscribers(portalId);

                while (reader.Read())
                {
                    yield return CBO.FillObject<Subscriber>(reader, false);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public DateTime? GetLastRunDate(int portalId)
        {
            return _dataService.GetLastRunDate(portalId);
        }

        #endregion
    }
}