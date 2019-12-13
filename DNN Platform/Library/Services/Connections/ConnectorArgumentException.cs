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
