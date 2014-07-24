using log4net.Core;
using System;

namespace log4net.Util
{
	public class OnlyOnceErrorHandler : IErrorHandler
	{
		private DateTime m_enabledDate;

		private bool m_firstTime = true;

		private string m_message;

		private System.Exception m_exception;

		private log4net.Core.ErrorCode m_errorCode;

		private readonly string m_prefix;

		private readonly static Type declaringType;

		public DateTime EnabledDate
		{
			get
			{
				return this.m_enabledDate;
			}
		}

		public log4net.Core.ErrorCode ErrorCode
		{
			get
			{
				return this.m_errorCode;
			}
		}

		public string ErrorMessage
		{
			get
			{
				return this.m_message;
			}
		}

		public System.Exception Exception
		{
			get
			{
				return this.m_exception;
			}
		}

		public bool IsEnabled
		{
			get
			{
				return this.m_firstTime;
			}
		}

		static OnlyOnceErrorHandler()
		{
			OnlyOnceErrorHandler.declaringType = typeof(OnlyOnceErrorHandler);
		}

		public OnlyOnceErrorHandler()
		{
			this.m_prefix = "";
		}

		public OnlyOnceErrorHandler(string prefix)
		{
			this.m_prefix = prefix;
		}

		public void Error(string message, System.Exception e, log4net.Core.ErrorCode errorCode)
		{
			if (this.m_firstTime)
			{
				this.m_enabledDate = DateTime.Now;
				this.m_errorCode = errorCode;
				this.m_exception = e;
				this.m_message = message;
				this.m_firstTime = false;
				if (LogLog.InternalDebugging && !LogLog.QuietMode)
				{
					Type type = OnlyOnceErrorHandler.declaringType;
					string[] mPrefix = new string[] { "[", this.m_prefix, "] ErrorCode: ", errorCode.ToString(), ". ", message };
					LogLog.Error(type, string.Concat(mPrefix), e);
				}
			}
		}

		public void Error(string message, System.Exception e)
		{
			this.Error(message, e, log4net.Core.ErrorCode.GenericFailure);
		}

		public void Error(string message)
		{
			this.Error(message, null, log4net.Core.ErrorCode.GenericFailure);
		}

		public void Reset()
		{
			this.m_enabledDate = DateTime.MinValue;
			this.m_errorCode = log4net.Core.ErrorCode.GenericFailure;
			this.m_exception = null;
			this.m_message = null;
			this.m_firstTime = true;
		}
	}
}