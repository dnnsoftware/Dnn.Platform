#region Usings

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class SchedulerException : BasePortalException
    {
        //default constructor
		public SchedulerException()
        {
        }

        //constructor with exception message
		public SchedulerException(string message) : base(message)
        {
        }

		//constructor with message and inner exception
        public SchedulerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SchedulerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
