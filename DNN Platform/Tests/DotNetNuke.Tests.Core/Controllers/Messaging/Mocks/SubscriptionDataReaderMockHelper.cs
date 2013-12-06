#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
