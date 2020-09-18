// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System.Data.SqlClient;
    using System.Text;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The SqlUtils class provides Shared/Static methods for working with SQL Server related code.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SqlUtils
    {
        /// <summary>
        ///   function to translate sql exceptions to readable messages.
        ///   It also captures cases where sql server is not available and guards against
        ///   database connection details being leaked.
        /// </summary>
        /// <param name = "exc"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string TranslateSQLException(SqlException exc)
        {
            int i = 0;
            var errorMessages = new StringBuilder();
            for (i = 0; i <= exc.Errors.Count - 1; i++)
            {
                SqlError sqlError = exc.Errors[i];
                string filteredMessage = string.Empty;
                switch (sqlError.Number)
                {
                    case 17:
                        filteredMessage = "Sql server does not exist or access denied";
                        break;
                    case 4060:
                        filteredMessage = "Invalid Database";
                        break;
                    case 18456:
                        filteredMessage = "Sql login failed";
                        break;
                    case 1205:
                        filteredMessage = "Sql deadlock victim";
                        break;
                    default:
                        filteredMessage = exc.ToString();
                        break;
                }

                errorMessages.Append("<b>Index #:</b> " + i + "<br/>" + "<b>Source:</b> " + sqlError.Source + "<br/>" + "<b>Class:</b> " + sqlError.Class + "<br/>" + "<b>Number:</b> " +
                                     sqlError.Number + "<br/>" + "<b>Procedure:</b> " + sqlError.Procedure + "<br/>" + "<b>Message:</b> " + filteredMessage + "<br/>");
            }

            return errorMessages.ToString();
        }
    }
}
