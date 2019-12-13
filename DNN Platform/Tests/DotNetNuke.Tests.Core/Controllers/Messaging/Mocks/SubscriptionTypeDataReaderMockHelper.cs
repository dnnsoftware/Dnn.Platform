﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Data;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    internal class SubscriptionTypeDataReaderMockHelper
    {
        internal static IDataReader CreateEmptySubscriptionTypeReader()
        {
            return CreateSubscriptionTypeTable().CreateDataReader();
        }

        private static DataTable CreateSubscriptionTypeTable()
        {
            var table = new DataTable();

            var idColumn = table.Columns.Add("SubscriptionTypeId", typeof(int));
            table.Columns.Add("SubscriptionName", typeof(string));
            table.Columns.Add("FriendlyName", typeof(string));
            table.Columns.Add("DesktopModuleId", typeof(int));

            table.PrimaryKey = new[] { idColumn };

            return table;
        }
    }
}
