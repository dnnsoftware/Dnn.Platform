// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Data;
    using DotNetNuke.Tests.Utilities;

    public class DataUtil
    {
        public static void CreateDatabase(string databaseName)
        {
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }

            var engine = new SqlCeEngine(GetConnectionString(databaseName));
            engine.CreateDatabase();
            engine.Dispose();
        }

        public static void DeleteDatabase(string databaseName)
        {
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }
        }

        public static void ExecuteNonQuery(string databaseName, string sqlScript)
        {
            ExecuteNonQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => cmd.ExecuteNonQuery());
        }

        public static int ExecuteScalar(string databaseName, string sqlScript)
        {
            return ExecuteQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => (int)cmd.ExecuteScalar());
        }

        public static string GetConnectionString(string databaseName)
        {
            return string.Format("Data Source = {0};", databaseName);
        }

        public static T GetFieldValue<T>(string databaseName, string tableName, string field, string primaryKeyField, string primaryKeyValue)
        {
            return ExecuteQuery(databaseName, string.Format(DataResources.GetFieldValue, field, tableName, primaryKeyField, primaryKeyValue), (cmd) => (T)cmd.ExecuteScalar());
        }

        public static int GetLastAddedRecordID(string databaseName, string tableName, string primaryKeyField)
        {
            return ExecuteScalar(databaseName, string.Format(DataResources.GetLastAddedRecordID, tableName, primaryKeyField));
        }

        public static int GetRecordCount(string databaseName, string tableName)
        {
            return ExecuteScalar(databaseName, string.Format(DataResources.RecordCountScript, tableName));
        }

        public static int GetRecordCount(string databaseName, string tableName, string field, string value)
        {
            return ExecuteScalar(databaseName, string.Format(DataResources.GetRecordCountByField, tableName, field, value));
        }

        public static DataTable GetTable(string databaseName, string tableName)
        {
            return ExecuteQuery<DataTable>(databaseName, string.Format(DataResources.GetTable, tableName),
                                            (cmd) =>
                                            {
                                                var reader = cmd.ExecuteReader();
                                                var table = new DataTable();
                                                table.Load(reader);
                                                return table;
                                            });
        }

        public static void SetUpDatabase(int count)
        {
            string[] _dogAges = Constants.PETAPOCO_DogAges.Split(',');
            string[] _dogNames = Constants.PETAPOCO_DogNames.Split(',');

            CreateDatabase(Constants.PETAPOCO_DatabaseName);
            ExecuteNonQuery(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_CreateDogTableSql);
            var index = 0;
            for (int i = 0; i < count; i++)
            {
                if (index == 5)
                {
                    index = 0;
                }

                ExecuteNonQuery(Constants.PETAPOCO_DatabaseName, string.Format(Constants.PETAPOCO_InsertDogRow, _dogNames[index], _dogAges[index]));
                index++;
            }
        }

        private static TReturn ExecuteQuery<TReturn>(string databaseName, string sqlScript, Func<SqlCeCommand, TReturn> command)
        {
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    return command(cmd);
                }
            }
        }

        private static void ExecuteNonQuery(string databaseName, string sqlScript, Action<SqlCeCommand> command)
        {
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    command(cmd);
                }
            }
        }

        private static string ReplaceTokens(string sql)
        {
            return sql.Replace("{databaseOwner}", DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}
