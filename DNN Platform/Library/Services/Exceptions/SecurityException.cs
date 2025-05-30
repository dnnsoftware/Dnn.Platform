// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Xml.Serialization;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.UserRequest;

    public class SecurityException : BasePortalException
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityException));
        private string ip;
        private string querystring;

        // default constructor

        /// <summary>Initializes a new instance of the <see cref="SecurityException"/> class.</summary>
        public SecurityException()
        {
        }

        // constructor with exception message

        /// <summary>Initializes a new instance of the <see cref="SecurityException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public SecurityException(string message)
            : base(message)
        {
            this.InitializePrivateVariables();
        }

        // constructor with message and inner exception

        /// <summary>Initializes a new instance of the <see cref="SecurityException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public SecurityException(string message, Exception inner)
            : base(message, inner)
        {
            this.InitializePrivateVariables();
        }

        /// <summary>Initializes a new instance of the <see cref="SecurityException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected SecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.InitializePrivateVariables();
            this.ip = info.GetString("m_IP");
            this.querystring = info.GetString("m_Querystring");
        }

        [XmlElement("IP")]
        public string IP
        {
            get
            {
                return this.ip;
            }
        }

        [XmlElement("Querystring")]
        public string Querystring
        {
            get
            {
                return this.querystring;
            }
        }

        private void InitializePrivateVariables()
        {
            // Try and get the Portal settings from httpcontext
            try
            {
                var userRequestIpAddressController = UserRequestIPAddressController.Instance;
                var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));

                if (ipAddress != null)
                {
                    this.ip = ipAddress;
                }

                this.querystring = HttpContext.Current.Request.MapPath(this.Querystring, HttpContext.Current.Request.ApplicationPath, false);
            }
            catch (Exception exc)
            {
                this.ip = string.Empty;
                this.querystring = string.Empty;
                Logger.Error(exc);
            }
        }

        // public override void GetObjectData(SerializationInfo info, StreamingContext context)
        // {
        //    //Serialize this class' state and then call the base class GetObjectData
        //    info.AddValue("m_IP", m_IP, typeof (string));
        //    info.AddValue("m_Querystring", m_Querystring, typeof (string));
        //    base.GetObjectData(info, context);
        // }
    }
}
