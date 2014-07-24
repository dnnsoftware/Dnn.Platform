using log4net.Util;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;

namespace log4net.Core
{
	[Serializable]
	public class LocationInfo
	{
		private const string NA = "?";

		private readonly string m_className;

		private readonly string m_fileName;

		private readonly string m_lineNumber;

		private readonly string m_methodName;

		private readonly string m_fullInfo;

		private readonly StackFrame[] m_stackFrames;

		private readonly static Type declaringType;

		public string ClassName
		{
			get
			{
				return this.m_className;
			}
		}

		public string FileName
		{
			get
			{
				return this.m_fileName;
			}
		}

		public string FullInfo
		{
			get
			{
				return this.m_fullInfo;
			}
		}

		public string LineNumber
		{
			get
			{
				return this.m_lineNumber;
			}
		}

		public string MethodName
		{
			get
			{
				return this.m_methodName;
			}
		}

		public StackFrame[] StackFrames
		{
			get
			{
				return this.m_stackFrames;
			}
		}

		static LocationInfo()
		{
			LocationInfo.declaringType = typeof(LocationInfo);
		}

		public LocationInfo(Type callerStackBoundaryDeclaringType)
		{
			this.m_className = "?";
			this.m_fileName = "?";
			this.m_lineNumber = "?";
			this.m_methodName = "?";
			this.m_fullInfo = "?";
			if (callerStackBoundaryDeclaringType != null)
			{
				try
				{
					StackTrace stackTrace = new StackTrace(true);
					int num = 0;
					while (true)
					{
						if (num < stackTrace.FrameCount)
						{
							StackFrame frame = stackTrace.GetFrame(num);
							if (frame != null && frame.GetMethod().DeclaringType == callerStackBoundaryDeclaringType)
							{
								break;
							}
							num++;
						}
						else
						{
							break;
						}
					}
					while (num < stackTrace.FrameCount)
					{
						StackFrame stackFrame = stackTrace.GetFrame(num);
						if (stackFrame != null && stackFrame.GetMethod().DeclaringType != callerStackBoundaryDeclaringType)
						{
							break;
						}
						num++;
					}
					if (num < stackTrace.FrameCount)
					{
						int frameCount = stackTrace.FrameCount - num;
						ArrayList arrayLists = new ArrayList(frameCount);
						this.m_stackFrames = new StackFrame[frameCount];
						for (int i = num; i < stackTrace.FrameCount; i++)
						{
							arrayLists.Add(stackTrace.GetFrame(i));
						}
						arrayLists.CopyTo(this.m_stackFrames, 0);
						StackFrame frame1 = stackTrace.GetFrame(num);
						if (frame1 != null)
						{
							MethodBase method = frame1.GetMethod();
							if (method != null)
							{
								this.m_methodName = method.Name;
								if (method.DeclaringType != null)
								{
									this.m_className = method.DeclaringType.FullName;
								}
							}
							this.m_fileName = frame1.GetFileName();
							int fileLineNumber = frame1.GetFileLineNumber();
							this.m_lineNumber = fileLineNumber.ToString(NumberFormatInfo.InvariantInfo);
							object[] mClassName = new object[] { this.m_className, '.', this.m_methodName, '(', this.m_fileName, ':', this.m_lineNumber, ')' };
							this.m_fullInfo = string.Concat(mClassName);
						}
					}
				}
				catch (SecurityException securityException)
				{
					LogLog.Debug(LocationInfo.declaringType, "Security exception while trying to get caller stack frame. Error Ignored. Location Information Not Available.");
				}
			}
		}

		public LocationInfo(string className, string methodName, string fileName, string lineNumber)
		{
			this.m_className = className;
			this.m_fileName = fileName;
			this.m_lineNumber = lineNumber;
			this.m_methodName = methodName;
			object[] mClassName = new object[] { this.m_className, '.', this.m_methodName, '(', this.m_fileName, ':', this.m_lineNumber, ')' };
			this.m_fullInfo = string.Concat(mClassName);
		}
	}
}