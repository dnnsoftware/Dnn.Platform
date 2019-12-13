#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class ObjectHydrationException : BasePortalException
    {
        private List<string> _Columns;
        private Type _Type;

        public ObjectHydrationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ObjectHydrationException(string message, Exception innerException, Type type, IDataReader dr) : base(message, innerException)
        {
            _Type = type;
            _Columns = new List<string>();
            foreach (DataRow row in dr.GetSchemaTable().Rows)
            {
                _Columns.Add(row["ColumnName"].ToString());
            }
        }

        protected ObjectHydrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public List<string> Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                _Columns = value;
            }
        }

        public Type Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        public override string Message
        {
            get
            {
                string _Message = base.Message;
                _Message += " Expecting - " + Type + ".";
                _Message += " Returned - ";
                foreach (string columnName in Columns)
                {
                    _Message += columnName + ", ";
                }
                return _Message;
            }
        }
    }
}
