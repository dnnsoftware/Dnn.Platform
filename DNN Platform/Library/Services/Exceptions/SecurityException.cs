// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web;
    using System.Xml.Serialization;

    using DotNetNuke.Instrumentation;

    public class SecurityException : BasePortalException
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityException));
        private string m_IP;
        private string m_Querystring;

        // default constructor
        public SecurityException()
        {
        }

        // constructor with exception message
        public SecurityException(string message)
            : base(message)
        {
            this.InitilizePrivateVariables();
        }

        // constructor with message and inner exception
        public SecurityException(string message, Exception inner)
            : base(message, inner)
        {
            this.InitilizePrivateVariables();
        }

        protected SecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.InitilizePrivateVariables();
            this.m_IP = info.GetString("m_IP");
            this.m_Querystring = info.GetString("m_Querystring");
        }

        [XmlElement("IP")]
        public string IP
        {
            get
            {
                return this.m_IP;
            }
        }

        [XmlElement("Querystring")]
        public string Querystring
        {
            get
            {
                return this.m_Querystring;
            }
        }

        private void InitilizePrivateVariables()
        {
            // Try and get the Portal settings from httpcontext
            try
            {
                if (HttpContext.Current.Request.UserHostAddress != null)
                {
                    this.m_IP = HttpContext.Current.Request.UserHostAddress;
                }

                this.m_Querystring = HttpContext.Current.Request.MapPath(this.Querystring, HttpContext.Current.Request.ApplicationPath, false);
            }
            catch (Exception exc)
            {
                this.m_IP = string.Empty;
                this.m_Querystring = string.Empty;
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
