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
        private List<string> columns;
        private Type type;

        /// <summary>Initializes a new instance of the <see cref="ObjectHydrationException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public ObjectHydrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectHydrationException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        /// <param name="type">The type being hydrated.</param>
        /// <param name="dr">The data reader (for getting column names).</param>
        public ObjectHydrationException(string message, Exception innerException, Type type, IDataReader dr)
            : base(message, innerException)
        {
            this.type = type;
            this.columns = new List<string>();
            foreach (DataRow row in dr.GetSchemaTable().Rows)
            {
                this.columns.Add(row["ColumnName"].ToString());
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectHydrationException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ObjectHydrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                string message = base.Message;
                message += " Expecting - " + this.Type + ".";
                message += " Returned - ";
                foreach (string columnName in this.Columns)
                {
                    message += columnName + ", ";
                }

                return message;
            }
        }

        public List<string> Columns
        {
            get
            {
                return this.columns;
            }

            set
            {
                this.columns = value;
            }
        }

        public Type Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }
    }
}
