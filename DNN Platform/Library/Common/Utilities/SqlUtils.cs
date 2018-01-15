#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
#region Usings

using System.Data.SqlClient;
using System.Text;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The SqlUtils class provides Shared/Static methods for working with SQL Server related code
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SqlUtils
    {
        #region "Public Methods"

        #endregion

        /// <summary>
        ///   function to translate sql exceptions to readable messages. 
        ///   It also captures cases where sql server is not available and guards against
        ///   database connection details being leaked
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
