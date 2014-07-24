using System.Text.RegularExpressions;
using System.Xml;

namespace log4net.Util
{
	public sealed class Transform
	{
		private const string CDATA_END = "]]>";

		private const string CDATA_UNESCAPABLE_TOKEN = "]]";

		private static Regex INVALIDCHARS;

		static Transform()
		{
			Transform.INVALIDCHARS = new Regex("[^\\x09\\x0A\\x0D\\x20-\\xFF\\u00FF-\\u07FF\\uE000-\\uFFFD]", RegexOptions.Compiled);
		}

		private Transform()
		{
		}

		private static int CountSubstrings(string text, string substring)
		{
			int num = 0;
			int num1 = 0;
			int length = text.Length;
			int length1 = substring.Length;
			if (length == 0)
			{
				return 0;
			}
			if (length1 == 0)
			{
				return 0;
			}
			while (num1 < length)
			{
				int num2 = text.IndexOf(substring, num1);
				if (num2 == -1)
				{
					break;
				}
				num++;
				num1 = num2 + length1;
			}
			return num;
		}

		public static string MaskXmlInvalidCharacters(string textData, string mask)
		{
			return Transform.INVALIDCHARS.Replace(textData, mask);
		}

		public static void WriteEscapedXmlString(XmlWriter writer, string textData, string invalidCharReplacement)
		{
			string str = Transform.MaskXmlInvalidCharacters(textData, invalidCharReplacement);
			int num = 12 * (1 + Transform.CountSubstrings(str, "]]>"));
			if (3 * (Transform.CountSubstrings(str, "<") + Transform.CountSubstrings(str, ">")) + 4 * Transform.CountSubstrings(str, "&") <= num)
			{
				writer.WriteString(str);
				return;
			}
			int num1 = str.IndexOf("]]>");
			if (num1 < 0)
			{
				writer.WriteCData(str);
				return;
			}
			int length = 0;
			while (num1 > -1)
			{
				writer.WriteCData(str.Substring(length, num1 - length));
				if (num1 != str.Length - 3)
				{
					writer.WriteString("]]");
					length = num1 + 2;
					num1 = str.IndexOf("]]>", length);
				}
				else
				{
					length = str.Length;
					writer.WriteString("]]>");
					break;
				}
			}
			if (length < str.Length)
			{
				writer.WriteCData(str.Substring(length));
			}
		}
	}
}