using System;

namespace log4net.Core
{
	public interface IErrorHandler
	{
		void Error(string message, Exception e, ErrorCode errorCode);

		void Error(string message, Exception e);

		void Error(string message);
	}
}