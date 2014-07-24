using log4net.Util;
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace log4net.Layout.Pattern
{
	internal class StackTraceDetailPatternConverter : StackTracePatternConverter
	{
		private readonly static Type declaringType;

		static StackTraceDetailPatternConverter()
		{
			StackTraceDetailPatternConverter.declaringType = typeof(StackTracePatternConverter);
		}

		public StackTraceDetailPatternConverter()
		{
		}

		internal override string GetMethodInformation(MethodBase method)
		{
			string str = "";
			try
			{
				string str1 = "";
				string[] methodParameterNames = this.GetMethodParameterNames(method);
				StringBuilder stringBuilder = new StringBuilder();
				if (methodParameterNames != null && methodParameterNames.GetUpperBound(0) > 0)
				{
					for (int i = 0; i <= methodParameterNames.GetUpperBound(0); i++)
					{
						stringBuilder.AppendFormat("{0}, ", methodParameterNames[i]);
					}
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Remove(stringBuilder.Length - 2, 2);
					str1 = stringBuilder.ToString();
				}
				str = string.Concat(base.GetMethodInformation(method), "(", str1, ")");
			}
			catch (Exception exception)
			{
				LogLog.Error(StackTraceDetailPatternConverter.declaringType, "An exception ocurred while retreiving method information.", exception);
			}
			return str;
		}

		private string[] GetMethodParameterNames(MethodBase methodBase)
		{
			ArrayList arrayLists = new ArrayList();
			try
			{
				ParameterInfo[] parameters = methodBase.GetParameters();
				int upperBound = parameters.GetUpperBound(0);
				for (int i = 0; i <= upperBound; i++)
				{
					arrayLists.Add(string.Concat(parameters[i].ParameterType, " ", parameters[i].Name));
				}
			}
			catch (Exception exception)
			{
				LogLog.Error(StackTraceDetailPatternConverter.declaringType, "An exception ocurred while retreiving method parameters.", exception);
			}
			return (string[])arrayLists.ToArray(typeof(string));
		}
	}
}