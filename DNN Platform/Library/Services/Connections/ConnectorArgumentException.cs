// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.Connections
{
    public class ConnectorArgumentException : ApplicationException
    {
        public ConnectorArgumentException()
        {
            
        }

        public ConnectorArgumentException(string message) : base(message)
        {
            
        }

        public ConnectorArgumentException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
