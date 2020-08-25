// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

    using DotNetNuke.Services.Social.Messaging;

    internal class UserPreferenceDataReaderMockHelper
    {
        internal static IDataReader CreateEmptyUserPreferenceReader()
        {
            return CreateUserPreferenceTable().CreateDataReader();
        }

        internal static IDataReader CreateUserPreferenceReader(UserPreference userPreference)
        {
            var table = CreateUserPreferenceTable();
            AddUserPreferenceToTable(table, userPreference);
            return table.CreateDataReader();
        }

        private static DataTable CreateUserPreferenceTable()
        {
            var table = new DataTable();

            var idColumn = table.Columns.Add("UserPreferenceId", typeof(int));
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("UserId", typeof(string));
            table.Columns.Add("MessagesEmailFrequency", typeof(int));
            table.Columns.Add("NotificationsEmailFrequency", typeof(int));

            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static void AddUserPreferenceToTable(DataTable table, UserPreference userPreference)
        {
            table.Rows.Add(new object[]
            {
                1,
                userPreference.PortalId,
                userPreference.UserId,
                userPreference.MessagesEmailFrequency,
                userPreference.NotificationsEmailFrequency,
            });
        }
    }
}
