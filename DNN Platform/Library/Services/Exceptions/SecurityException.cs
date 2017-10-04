#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Xml.Serialization;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class SecurityException : BasePortalException
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SecurityException));
        private string m_IP;
        private string m_Querystring;

        //default constructor
        public SecurityException()
        {
        }

        //constructor with exception message
        public SecurityException(string message) : base(message)
        {
            InitilizePrivateVariables();
        }

        //constructor with message and inner exception
        public SecurityException(string message, Exception inner) : base(message, inner)
        {
            InitilizePrivateVariables();
        }

        protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InitilizePrivateVariables();
            m_IP = info.GetString("m_IP");
            m_Querystring = info.GetString("m_Querystring");
        }

        [XmlElement("IP")]
        public string IP
        {
            get
            {
                return m_IP;
            }
        }

        [XmlElement("Querystring")]
        public string Querystring
        {
            get
            {
                return m_Querystring;
            }
        }

        private void InitilizePrivateVariables()
        {
			//Try and get the Portal settings from httpcontext
            try
            {
                if (HttpContext.Current.Request.UserHostAddress != null)
                {
                    m_IP = HttpContext.Current.Request.UserHostAddress;
                }
                m_Querystring = HttpContext.Current.Request.MapPath(Querystring, HttpContext.Current.Request.ApplicationPath, false);
            }
            catch (Exception exc)
            {
                m_IP = "";
                m_Querystring = "";
                Logger.Error(exc);

            }
        }

        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    //Serialize this class' state and then call the base class GetObjectData
        //    info.AddValue("m_IP", m_IP, typeof (string));
        //    info.AddValue("m_Querystring", m_Querystring, typeof (string));
        //    base.GetObjectData(info, context);
        //}
    }
}