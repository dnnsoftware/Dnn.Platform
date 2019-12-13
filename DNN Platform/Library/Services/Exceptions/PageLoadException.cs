// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class PageLoadException : BasePortalException
    {
        //default constructor
		public PageLoadException()
        {
        }

		//constructor with exception message
        public PageLoadException(string message) : base(message)
        {
        }

		//constructor with message and inner exception
        public PageLoadException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PageLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
