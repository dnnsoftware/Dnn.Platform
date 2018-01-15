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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Xml.Serialization;

using DotNetNuke.Application;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Exceptions
{
	/// <summary>
	/// Base Portal Exception.
	/// </summary>
    public class BasePortalException : Exception
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (BasePortalException));
	    private string m_InnerExceptionString;
        private string m_Message;
	    private string m_Source;
        private string m_StackTrace;

	    //default constructor
		public BasePortalException()
        {
        }

        //constructor with exception message
		public BasePortalException(string message) : base(message)
        {
            InitializePrivateVariables();
        }

        //constructor with message and inner exception
        public BasePortalException(string message, Exception inner) : base(message, inner)
        {
            InitializePrivateVariables();
        }

        protected BasePortalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InitializePrivateVariables();
            AssemblyVersion = info.GetString("m_AssemblyVersion");
            PortalID = info.GetInt32("m_PortalID");
            PortalName = info.GetString("m_PortalName");
            UserID = info.GetInt32("m_UserID");
            UserName = info.GetString("m_Username");
            ActiveTabID = info.GetInt32("m_ActiveTabID");
            ActiveTabName = info.GetString("m_ActiveTabName");
            RawURL = info.GetString("m_RawURL");
            AbsoluteURL = info.GetString("m_AbsoluteURL");
            AbsoluteURLReferrer = info.GetString("m_AbsoluteURLReferrer");
            UserAgent = info.GetString("m_UserAgent");
            DefaultDataProvider = info.GetString("m_DefaultDataProvider");
            ExceptionGUID = info.GetString("m_ExceptionGUID");
            m_InnerExceptionString = info.GetString("m_InnerExceptionString");
            FileName = info.GetString("m_FileName");
            FileLineNumber = info.GetInt32("m_FileLineNumber");
            FileColumnNumber = info.GetInt32("m_FileColumnNumber");
            Method = info.GetString("m_Method");
            m_StackTrace = info.GetString("m_StackTrace");
            m_Message = info.GetString("m_Message");
            m_Source = info.GetString("m_Source");
        }

	    public string AssemblyVersion { get; private set; }

	    public int PortalID { get; private set; }

	    public string PortalName { get; private set; }

	    public int UserID { get; private set; }

	    public string UserName { get; private set; }

	    public int ActiveTabID { get; private set; }

	    public string ActiveTabName { get; private set; }

	    public string RawURL { get; private set; }

	    public string AbsoluteURL { get; private set; }

	    public string AbsoluteURLReferrer { get; private set; }

	    public string UserAgent { get; private set; }

	    public string DefaultDataProvider { get; private set; }

	    public string ExceptionGUID { get; private set; }

	    public string FileName { get; private set; }

	    public int FileLineNumber { get; private set; }

	    public int FileColumnNumber { get; private set; }

	    public string Method { get; private set; }

	    [XmlIgnore]
        public new MethodBase TargetSite
        {
            get
            {
                return base.TargetSite;
            }
        }

        private void InitializePrivateVariables()
        {
			//Try and get the Portal settings from context
            //If an error occurs getting the context then set the variables to -1
            try
            {
                var context = HttpContext.Current;
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                var innerException = new Exception(Message, this);
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                var exceptionInfo = Exceptions.GetExceptionInfo(innerException);

                AssemblyVersion = DotNetNukeContext.Current.Application.Version.ToString(3);
                if (portalSettings != null)
                {
                    PortalID = portalSettings.PortalId;
                    PortalName = portalSettings.PortalName;
                    ActiveTabID = portalSettings.ActiveTab.TabID;
                    ActiveTabName = portalSettings.ActiveTab.TabName;
                }
                else
                {
                    PortalID = -1;
                    PortalName = "";
                    ActiveTabID = -1;
                    ActiveTabName = "";
                }

                var currentUserInfo = UserController.Instance.GetCurrentUserInfo();
                UserID = (currentUserInfo != null) ? currentUserInfo.UserID : -1;

                if (UserID != -1)
                {
                    currentUserInfo = UserController.GetUserById(PortalID, UserID);
                    UserName = currentUserInfo != null ? currentUserInfo.Username : "";
                }
                else
                {
                    UserName = "";
                }

                if (context != null)
                {
                    RawURL = context.Request.RawUrl;
                    AbsoluteURL = context.Request.Url.AbsolutePath;
                    if (context.Request.UrlReferrer != null)
                    {
                        AbsoluteURLReferrer = context.Request.UrlReferrer.AbsoluteUri;
                    }
                    UserAgent = context.Request.UserAgent;
                }
                else
                {
                    RawURL = "";
                    AbsoluteURL = "";
                    AbsoluteURLReferrer = "";
                    UserAgent = "";
                }
                try
                {
                    ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
                    string strTypeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
                    DefaultDataProvider = strTypeName;
                    
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    DefaultDataProvider = "";
                }

                ExceptionGUID = Guid.NewGuid().ToString();

                if (exceptionInfo != null)
                {
                    FileName = exceptionInfo.FileName;
                    FileLineNumber = exceptionInfo.FileLineNumber;
                    FileColumnNumber = exceptionInfo.FileColumnNumber;
                    Method = exceptionInfo.Method;
                }
                else
                {
                    FileName = "";
                    FileLineNumber = -1;
                    FileColumnNumber = -1;
                    Method = "";
                }

                try
                {
                    m_StackTrace = StackTrace;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    m_StackTrace = "";
                }
                try
                {
                    m_Message = Message;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    m_Message = "";
                }
                try
                {
                    m_Source = Source;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    m_Source = "";
                }
            }
            catch (Exception exc)
            {
                PortalID = -1;
                UserID = -1;
                AssemblyVersion = "-1";
                ActiveTabID = -1;
                ActiveTabName = "";
                RawURL = "";
                AbsoluteURL = "";
                AbsoluteURLReferrer = "";
                UserAgent = "";
                DefaultDataProvider = "";
                ExceptionGUID = "";
                FileName = "";
                FileLineNumber = -1;
                FileColumnNumber = -1;
                Method = "";
                m_StackTrace = "";
                m_Message = "";
                m_Source = "";
                Logger.Error(exc);

            }
        }

        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    //Serialize this class' state and then call the base class GetObjectData
        //    info.AddValue("m_AssemblyVersion", AssemblyVersion, typeof (string));
        //    info.AddValue("m_PortalID", PortalID, typeof (Int32));
        //    info.AddValue("m_PortalName", PortalName, typeof (string));
        //    info.AddValue("m_UserID", UserID, typeof (Int32));
        //    info.AddValue("m_UserName", UserName, typeof (string));
        //    info.AddValue("m_ActiveTabID", ActiveTabID, typeof (Int32));
        //    info.AddValue("m_ActiveTabName", ActiveTabName, typeof (string));
        //    info.AddValue("m_RawURL", RawURL, typeof (string));
        //    info.AddValue("m_AbsoluteURL", AbsoluteURL, typeof (string));
        //    info.AddValue("m_AbsoluteURLReferrer", AbsoluteURLReferrer, typeof (string));
        //    info.AddValue("m_UserAgent", UserAgent, typeof (string));
        //    info.AddValue("m_DefaultDataProvider", DefaultDataProvider, typeof (string));
        //    info.AddValue("m_ExceptionGUID", ExceptionGUID, typeof (string));
        //    info.AddValue("m_FileName", FileName, typeof (string));
        //    info.AddValue("m_FileLineNumber", FileLineNumber, typeof (Int32));
        //    info.AddValue("m_FileColumnNumber", FileColumnNumber, typeof (Int32));
        //    info.AddValue("m_Method", Method, typeof (string));
        //    info.AddValue("m_StackTrace", m_StackTrace, typeof (string));
        //    info.AddValue("m_Message", m_Message, typeof (string));
        //    info.AddValue("m_Source", m_Source, typeof (string));
        //    base.GetObjectData(info, context);
        //}
    }
}