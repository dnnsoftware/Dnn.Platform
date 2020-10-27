// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    public class ObjectHydrationException : BasePortalException
    {
        private List<string> _Columns;
        private Type _Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectHydrationException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ObjectHydrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectHydrationException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="type"></param>
        /// <param name="dr"></param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectHydrationException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ObjectHydrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
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
