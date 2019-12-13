﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Data;
using DotNetNuke.Services.Social.Messaging;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
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
            table.Rows.Add(new object[] {
                1, 
                userPreference.PortalId,
                userPreference.UserId, 
                userPreference.MessagesEmailFrequency, 
                userPreference.NotificationsEmailFrequency,
            });
        }
    }
}
