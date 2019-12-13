// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    internal class SubscriptionDataReaderMockHelper
    {
        internal static IDataReader CreateEmptySubscriptionReader()
        {
            return CreateSubscriptionTable().CreateDataReader();
        }

        internal static IDataReader CreateSubscriptionReader(IEnumerable<Subscription> subscriptions)
        {
            var datatable = CreateSubscriptionTable();
            foreach (var subscription in subscriptions)
            {
                AddSubscriptionToTable(datatable, subscription);
            }
            return datatable.CreateDataReader();
        }

        private static DataTable CreateSubscriptionTable()
        {
            var table = new DataTable();

            var idColumn = table.Columns.Add("SubscriptionId", typeof(int));
            table.Columns.Add("UserId", typeof(int));
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("SubscriptionTypeId", typeof(int));
            table.Columns.Add("ObjectKey", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("ModuleID", typeof(int));
            table.Columns.Add("TabId", typeof(int));

            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static void AddSubscriptionToTable(DataTable table, Subscription subscription)
        {
            table.Rows.Add(new object[] {
                subscription.SubscriptionId, 
                subscription.UserId,
                subscription.PortalId, 
                subscription.SubscriptionTypeId, 
                subscription.ObjectKey,
                subscription.Description,
                subscription.CreatedOnDate,
                subscription.ModuleId,
                subscription.TabId
            });
        }
    }
}
