// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Data;

    using global::PetaPoco;

    public static class PetaPocoExt
    {
        public static IDataReader ExecuteReader(this Database database, string sql, params object[] args)
        {
            IDataReader reader;
            try
            {
                database.OpenSharedConnection();

                using (IDbCommand command = database.CreateCommand(database.Connection, sql, args))
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    database.OnExecutedCommand(command);
                }
            }
            catch (Exception exception)
            {
                if (database.OnException(exception))
                {
                    throw;
                }

                reader = null;
            }

            return reader;
        }
    }
}
