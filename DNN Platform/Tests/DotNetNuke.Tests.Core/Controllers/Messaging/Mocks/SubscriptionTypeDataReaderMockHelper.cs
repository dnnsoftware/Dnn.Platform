// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

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
