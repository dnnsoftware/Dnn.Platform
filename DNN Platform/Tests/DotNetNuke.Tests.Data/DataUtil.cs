#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Text.RegularExpressions;
using DotNetNuke.Data;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Data
{
    public class DataUtil
    {
        #region Public Methods

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
            return String.Format("Data Source = {0};", databaseName);
        }

        public static T GetFieldValue<T>(string databaseName, string tableName, string field, string primaryKeyField, string primaryKeyValue)
        {
            return ExecuteQuery(databaseName, String.Format(DataResources.GetFieldValue, field, tableName, primaryKeyField, primaryKeyValue), (cmd) => (T)cmd.ExecuteScalar());
        }

        public static int GetLastAddedRecordID(string databaseName, string tableName, string primaryKeyField)
        {
            return ExecuteScalar(databaseName, String.Format(DataResources.GetLastAddedRecordID, tableName, primaryKeyField));
        }

        public static int GetRecordCount(string databaseName, string tableName)
        {
            return ExecuteScalar(databaseName, String.Format(DataResources.RecordCountScript, tableName));
        }

        public static int GetRecordCount(string databaseName, string tableName, string field, string value)
        {
            return ExecuteScalar(databaseName, String.Format(DataResources.GetRecordCountByField, tableName, field, value));
        }

        public static DataTable GetTable(string databaseName, string tableName)
        {
            return ExecuteQuery<DataTable>(databaseName, String.Format(DataResources.GetTable, tableName),
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
                ExecuteNonQuery(Constants.PETAPOCO_DatabaseName, String.Format(Constants.PETAPOCO_InsertDogRow, _dogNames[index], _dogAges[index]));
                index++;
            }            
        }

        #endregion

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