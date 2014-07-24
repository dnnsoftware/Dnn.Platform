using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace log4net.Util
{
	public sealed class NativeError
	{
		private int m_number;

		private string m_message;

		public string Message
		{
			get
			{
				return this.m_message;
			}
		}

		public int Number
		{
			get
			{
				return this.m_number;
			}
		}

		private NativeError(int number, string message)
		{
			this.m_number = number;
			this.m_message = message;
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref string lpBuffer, int nSize, IntPtr Arguments);

		public static NativeError GetError(int number)
		{
			return new NativeError(number, NativeError.GetErrorMessage(number));
		}

		public static string GetErrorMessage(int messageId)
		{
			int num = 256;
			int num1 = 512;
			int num2 = 4096;
			string str = "";
			IntPtr intPtr = new IntPtr();
			IntPtr intPtr1 = new IntPtr();
			if (messageId == 0)
			{
				str = null;
			}
			else if (NativeError.FormatMessage(num | num2 | num1, ref intPtr, messageId, 0, ref str, 255, intPtr1) <= 0)
			{
				str = null;
			}
			else
			{
				char[] chrArray = new char[] { '\r', '\n' };
				str = str.TrimEnd(chrArray);
			}
			return str;
		}

		public static NativeError GetLastError()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			return new NativeError(lastWin32Error, NativeError.GetErrorMessage(lastWin32Error));
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] number = new object[] { this.Number };
			return string.Concat(string.Format(invariantCulture, "0x{0:x8}", number), (this.Message != null ? string.Concat(": ", this.Message) : ""));
		}
	}
}