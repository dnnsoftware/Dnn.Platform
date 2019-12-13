using System;
using System.Data;

using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
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
