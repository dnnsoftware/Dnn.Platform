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
