// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
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

    /// <summary>
    /// Base Portal Exception.
    /// </summary>
    public class BasePortalException : Exception
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BasePortalException));
        private string m_InnerExceptionString;
        private string m_Message;
        private string m_Source;
        private string m_StackTrace;

        // default constructor
        public BasePortalException()
        {
        }

        // constructor with exception message
        public BasePortalException(string message)
            : base(message)
        {
            this.InitializePrivateVariables();
        }

        // constructor with message and inner exception
        public BasePortalException(string message, Exception inner)
            : base(message, inner)
        {
            this.InitializePrivateVariables();
        }

        protected BasePortalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.InitializePrivateVariables();
            this.AssemblyVersion = info.GetString("m_AssemblyVersion");
            this.PortalID = info.GetInt32("m_PortalID");
            this.PortalName = info.GetString("m_PortalName");
            this.UserID = info.GetInt32("m_UserID");
            this.UserName = info.GetString("m_Username");
            this.ActiveTabID = info.GetInt32("m_ActiveTabID");
            this.ActiveTabName = info.GetString("m_ActiveTabName");
            this.RawURL = info.GetString("m_RawURL");
            this.AbsoluteURL = info.GetString("m_AbsoluteURL");
            this.AbsoluteURLReferrer = info.GetString("m_AbsoluteURLReferrer");
            this.UserAgent = info.GetString("m_UserAgent");
            this.DefaultDataProvider = info.GetString("m_DefaultDataProvider");
            this.ExceptionGUID = info.GetString("m_ExceptionGUID");
            this.m_InnerExceptionString = info.GetString("m_InnerExceptionString");
            this.FileName = info.GetString("m_FileName");
            this.FileLineNumber = info.GetInt32("m_FileLineNumber");
            this.FileColumnNumber = info.GetInt32("m_FileColumnNumber");
            this.Method = info.GetString("m_Method");
            this.m_StackTrace = info.GetString("m_StackTrace");
            this.m_Message = info.GetString("m_Message");
            this.m_Source = info.GetString("m_Source");
        }

        [XmlIgnore]
        public new MethodBase TargetSite
        {
            get
            {
                return base.TargetSite;
            }
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

        private void InitializePrivateVariables()
        {
            // Try and get the Portal settings from context
            // If an error occurs getting the context then set the variables to -1
            try
            {
                var context = HttpContext.Current;
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                var innerException = new Exception(this.Message, this);
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                var exceptionInfo = Exceptions.GetExceptionInfo(innerException);

                this.AssemblyVersion = DotNetNukeContext.Current.Application.Version.ToString(3);
                if (portalSettings != null)
                {
                    this.PortalID = portalSettings.PortalId;
                    this.PortalName = portalSettings.PortalName;
                    this.ActiveTabID = portalSettings.ActiveTab.TabID;
                    this.ActiveTabName = portalSettings.ActiveTab.TabName;
                }
                else
                {
                    this.PortalID = -1;
                    this.PortalName = string.Empty;
                    this.ActiveTabID = -1;
                    this.ActiveTabName = string.Empty;
                }

                var currentUserInfo = UserController.Instance.GetCurrentUserInfo();
                this.UserID = (currentUserInfo != null) ? currentUserInfo.UserID : -1;

                if (this.UserID != -1)
                {
                    currentUserInfo = UserController.GetUserById(this.PortalID, this.UserID);
                    this.UserName = currentUserInfo != null ? currentUserInfo.Username : string.Empty;
                }
                else
                {
                    this.UserName = string.Empty;
                }

                if (context != null)
                {
                    this.RawURL = HttpUtility.HtmlEncode(context.Request.RawUrl);
                    this.AbsoluteURL = HttpUtility.HtmlEncode(context.Request.Url.AbsolutePath);
                    if (context.Request.UrlReferrer != null)
                    {
                        this.AbsoluteURLReferrer = HttpUtility.HtmlEncode(context.Request.UrlReferrer.AbsoluteUri);
                    }

                    this.UserAgent = HttpUtility.HtmlEncode(context.Request.UserAgent ?? string.Empty);
                }
                else
                {
                    this.RawURL = string.Empty;
                    this.AbsoluteURL = string.Empty;
                    this.AbsoluteURLReferrer = string.Empty;
                    this.UserAgent = string.Empty;
                }

                try
                {
                    ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
                    string strTypeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
                    this.DefaultDataProvider = strTypeName;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    this.DefaultDataProvider = string.Empty;
                }

                this.ExceptionGUID = Guid.NewGuid().ToString();

                if (exceptionInfo != null)
                {
                    this.FileName = exceptionInfo.FileName;
                    this.FileLineNumber = exceptionInfo.FileLineNumber;
                    this.FileColumnNumber = exceptionInfo.FileColumnNumber;
                    this.Method = exceptionInfo.Method;
                }
                else
                {
                    this.FileName = string.Empty;
                    this.FileLineNumber = -1;
                    this.FileColumnNumber = -1;
                    this.Method = string.Empty;
                }

                try
                {
                    this.m_StackTrace = this.StackTrace;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    this.m_StackTrace = string.Empty;
                }

                try
                {
                    this.m_Message = this.Message;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    this.m_Message = string.Empty;
                }

                try
                {
                    this.m_Source = this.Source;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    this.m_Source = string.Empty;
                }
            }
            catch (Exception exc)
            {
                this.PortalID = -1;
                this.UserID = -1;
                this.AssemblyVersion = "-1";
                this.ActiveTabID = -1;
                this.ActiveTabName = string.Empty;
                this.RawURL = string.Empty;
                this.AbsoluteURL = string.Empty;
                this.AbsoluteURLReferrer = string.Empty;
                this.UserAgent = string.Empty;
                this.DefaultDataProvider = string.Empty;
                this.ExceptionGUID = string.Empty;
                this.FileName = string.Empty;
                this.FileLineNumber = -1;
                this.FileColumnNumber = -1;
                this.Method = string.Empty;
                this.m_StackTrace = string.Empty;
                this.m_Message = string.Empty;
                this.m_Source = string.Empty;
                Logger.Error(exc);
            }
        }

        // public override void GetObjectData(SerializationInfo info, StreamingContext context)
        // {
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
        // }
    }
}
