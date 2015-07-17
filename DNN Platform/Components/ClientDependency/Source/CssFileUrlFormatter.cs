using System;

namespace ClientDependency.Core
{
    [Obsolete("Use CssHelper instead")]
	public class CssFileUrlFormatter
	{
		public static string TransformCssFile(string fileContent, Uri cssLocation)
		{
		    return CssHelper.ReplaceUrlsWithAbsolutePaths(fileContent, cssLocation);
		}
	}
}
