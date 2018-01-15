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