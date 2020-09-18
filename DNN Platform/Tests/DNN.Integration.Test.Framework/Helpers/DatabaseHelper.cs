// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Dynamic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data.PetaPoco;

    public static class DatabaseHelper
    {
        private const string QualifierPrefix = "{objectQualifier}";

        private static IDictionary<string, object> ValuesDictionary
        {
            // case insensitive dictionary keys
            get { return new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase); }
        }

        public static bool CanConnectToDatabase()
        {
            try
            {
                PetaPocoHelper.ExecuteSQL(AppConfigHelper.ConnectionString, "SELECT GETDATE()");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static T ExecuteScalar<T>(string queryString, params object[] args)
        {
            var qstr = ReplaceQueryQualifier(queryString);
            return PetaPocoHelper.ExecuteScalar<T>(AppConfigHelper.ConnectionString, CommandType.Text, qstr, args);
        }

        public static IEnumerable<TItem> ExecuteQuery<TItem>(string queryString)
        {
            using (var connection = new SqlConnection(AppConfigHelper.ConnectionString))
            {
                connection.Open();
                queryString = ReplaceQueryQualifier(queryString);
                using (var command = new SqlCommand(queryString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        return CBO.FillCollection<TItem>(reader);
                    }
                }
            }
        }

        public static IList<IDictionary<string, object>> ExecuteQuery(string queryString)
        {
            var results = new List<IDictionary<string, object>>();
            using (var connection = new SqlConnection(AppConfigHelper.ConnectionString))
            {
                connection.Open();
                queryString = ReplaceQueryQualifier(queryString);
                using (var command = new SqlCommand(queryString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var values = ValuesDictionary;
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                values.Add(reader.GetName(i), reader.GetValue(i));
                            }

                            results.Add(values);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns a dynamic object with rows and columns.
        /// </summary>
        /// <returns>Row can be accessed by return[0]. Column can be accessed by return[0].ColumnName. Column names are case sensitive.</returns>
        public static IList<dynamic> ExecuteDynamicQuery(string queryString)
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(AppConfigHelper.ConnectionString))
            {
                connection.Open();
                queryString = ReplaceQueryQualifier(queryString);
                using (var command = new SqlCommand(queryString, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var values = new ExpandoObject() as IDictionary<string, object>;
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                values.Add(reader.GetName(i), reader.GetValue(i));

                                // values[reader.GetName(i)] = reader.GetValue(i);
                                // values.Add(reader.GetName(i), reader.GetValue(i));
                            }

                            results.Add(values);
                        }
                    }
                }
            }

            return results;
        }

        public static int ExecuteNonQuery(string queryString)
        {
            using (var connection = new SqlConnection(AppConfigHelper.ConnectionString))
            {
                connection.Open();
                queryString = ReplaceQueryQualifier(queryString);
                using (var command = new SqlCommand(queryString, connection))
                {
                    command.CommandType = CommandType.Text;
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a specific STORED PROCEDURE.
        /// The SP passed must not have the ObjectQualifier in its name; otherwise duplicate qualifier will be prefixed.
        /// </summary>
        /// <returns></returns>
        public static IList<IDictionary<string, object>> ExecuteStoredProcedure(string procedureName, params object[] sqlParameters)
        {
            var results = new List<IDictionary<string, object>>();
            procedureName = ReplaceQueryQualifier(AppConfigHelper.ObjectQualifier + procedureName);
            using (var reader = PetaPocoHelper.ExecuteReader(AppConfigHelper.ConnectionString, CommandType.StoredProcedure, procedureName, sqlParameters))
            {
                while (reader.Read())
                {
                    var values = ValuesDictionary;
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(reader.GetName(i), reader.GetValue(i));
                    }

                    results.Add(values);
                }
            }

            return results;
        }

        /// <summary>
        /// Executes a specific STORED PROCEDURE.
        /// The SP passed must not have the ObjectQualifier in its name; otherwise duplicate qualifier will be prefixed.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TItem> ExecuteStoredProcedure<TItem>(string procedureName, params object[] sqlParameters)
        {
            procedureName = ReplaceQueryQualifier(AppConfigHelper.ObjectQualifier + procedureName);
            return CBO.FillCollection<TItem>(PetaPocoHelper.ExecuteReader(AppConfigHelper.ConnectionString, CommandType.StoredProcedure, procedureName, sqlParameters));
        }

        /// <summary>
        /// Retrieves the total count of records of a table or view in the database.
        /// </summary>
        /// <remarks>DO NOT perfix the table/view name with a qualifier.</remarks>
        /// <returns></returns>
        public static int GetRecordsCount(string tableOrViewName, string whereCondition = null)
        {
            var queryString = ReplaceQueryQualifier(string.Format("SELECT COUNT(*) FROM [{0}{1}]", QualifierPrefix, tableOrViewName));

            if (!string.IsNullOrEmpty(whereCondition))
            {
                queryString += " WHERE " + whereCondition;
            }

            return PetaPocoHelper.ExecuteScalar<int>(AppConfigHelper.ConnectionString, CommandType.Text, queryString, null);
        }

        /// <summary>
        /// Retrieves the maximum ID of a column of a table or view in the database.
        /// </summary>
        /// <remarks>DO NOT perfix the table/view name with a qualifier.</remarks>
        /// <remarks>If no record exists in the table, -1 is returned.</remarks>
        /// <returns></returns>
        public static int GetLastRecordId(string tableOrViewName, string columnName, string whereCondition = null)
        {
            var query = string.Format(
                "SELECT COALESCE(MAX([{0}]), -1) FROM [{1}{2}]",
                columnName, QualifierPrefix, tableOrViewName);

            var queryString = ReplaceQueryQualifier(query);
            if (!string.IsNullOrEmpty(whereCondition))
            {
                queryString += " WHERE " + whereCondition;
            }

            return PetaPocoHelper.ExecuteScalar<int>(AppConfigHelper.ConnectionString, CommandType.Text, queryString, null);
        }

        public static IDictionary<string, object> GetRecordById(string tableOrViewName, string idColumnName, string idColumnValue)
        {
            var queryString = ReplaceQueryQualifier(
                string.Format("SELECT * FROM {0}{1} WHERE {2}='{3}'", QualifierPrefix, tableOrViewName, idColumnName, idColumnValue));

            var values = ValuesDictionary;

            using (var reader = PetaPocoHelper.ExecuteReader(AppConfigHelper.ConnectionString, CommandType.TableDirect, queryString))
            {
                while (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(reader.GetName(i), reader.GetValue(i));
                    }

                    break; // get the first record only
                }
            }

            return values;
        }

        private static string ReplaceQueryQualifier(string input)
        {
            return input.Replace(QualifierPrefix, AppConfigHelper.ObjectQualifier);
        }
    }
}
