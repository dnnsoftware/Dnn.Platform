// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    public class ObjectHydrationException : BasePortalException
    {
        private List<string> _Columns;
        private Type _Type;

        public ObjectHydrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ObjectHydrationException(string message, Exception innerException, Type type, IDataReader dr)
            : base(message, innerException)
        {
            this._Type = type;
            this._Columns = new List<string>();
            foreach (DataRow row in dr.GetSchemaTable().Rows)
            {
                this._Columns.Add(row["ColumnName"].ToString());
            }
        }

        protected ObjectHydrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Message
        {
            get
            {
                string _Message = base.Message;
                _Message += " Expecting - " + this.Type + ".";
                _Message += " Returned - ";
                foreach (string columnName in this.Columns)
                {
                    _Message += columnName + ", ";
                }

                return _Message;
            }
        }

        public List<string> Columns
        {
            get
            {
                return this._Columns;
            }

            set
            {
                this._Columns = value;
            }
        }

        public Type Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = value;
            }
        }
    }
}
