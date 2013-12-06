using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;
using System.Configuration.Provider;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core.CompositeFiles.Providers
{

	/// <summary>
	/// A provider for combining, minifying, compressing and saving composite scripts/css files
	/// </summary>
	public class CompositeFileProcessingProvider : BaseCompositeFileProcessingProvider
	{

	    public const string DefaultName = "CompositeFileProcessor";
		private static Regex _importCssRegex = new Regex("@import url\\((.+?)\\);?", RegexOptions.Compiled);

	    /// <summary>
	    /// Saves the file's bytes to disk with a hash of the byte array
	    /// </summary>
	    /// <param name="fileContents"></param>
	    /// <param name="type"></param>
	    /// <param name="server"></param>
	    /// <returns>The new file path</returns>
	    /// <remarks>
	    /// the extension will be: .cdj for JavaScript and .cdc for CSS
	    /// </remarks>
	    public override FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server, int version)
		{
            //don't save the file if composite files are disabled.
            if (!PersistCompositeFiles)
                return null;

            if (!CompositeFilePath.Exists)
                CompositeFilePath.Create();
			
            var fi = new FileInfo(
                Path.Combine(CompositeFilePath.FullName,
					version + "_" + Guid.NewGuid().ToString("N") + ".cd" + type.ToString().Substring(0, 1).ToUpper()));
			
            if (fi.Exists)
				fi.Delete();
			
            var fs = fi.Create();
			fs.Write(fileContents, 0, fileContents.Length);
			fs.Close();
			return fi;
		}

	    /// <summary>
	    /// combines all files to a byte array
	    /// </summary>
	    /// <param name="filePaths"></param>
	    /// <param name="context"></param>
	    /// <param name="type"></param>
	    /// <param name="fileDefs"></param>
	    /// <returns></returns>
	    public override byte[] CombineFiles(string[] filePaths, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs)
		{

			var fDefs = new List<CompositeFileDefinition>();

			var ms = new MemoryStream(5000);            
            var sw = new StreamWriter(ms, Encoding.UTF8);

			foreach (string s in filePaths)
			{
                sw.WriteLine("/* - Start of "+ s + " - */");
				if (!string.IsNullOrEmpty(s))
				{
					try
					{
						var fi = new FileInfo(context.Server.MapPath(s));
						if (ClientDependencySettings.Instance.FileBasedDependencyExtensionList.Contains(fi.Extension.ToUpper()))
						{
							//if the file doesn't exist, then we'll assume it is a URI external request
							if (!fi.Exists)
							{
                                WriteFileToStream(ref sw, s, type, ref fDefs, context);
							}
							else
							{
                                WriteFileToStream(ref sw, fi, type, s, ref fDefs, context);
							}
						}
						else
						{
							//if it's not a file based dependency, try to get the request output.
                            WriteFileToStream(ref sw, s, type, ref fDefs, context);
						}
					}
					catch (Exception ex)
					{
						Type exType = ex.GetType();
						if (exType.Equals(typeof(NotSupportedException)) 
							|| exType.Equals(typeof(ArgumentException))
							|| exType.Equals(typeof(HttpException)))
						{
							//could not parse the string into a fileinfo or couldn't mappath, so we assume it is a URI
                            WriteFileToStream(ref sw, s, type, ref fDefs, context);
						}
						else
						{
							//if this fails, log the exception in trace, but continue
                            ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", s, ex.Message), ex);
						}
					}
				}

				if (type == ClientDependencyType.Javascript)
				{
					sw.Write(";;;"); //write semicolons in case the js isn't formatted correctly. This also helps for debugging.
				}

                sw.WriteLine("/* - End of " + s + " - */");
			}
			sw.Flush();
			byte[] outputBytes = ms.ToArray();
			sw.Close();
			ms.Close();
			fileDefs = fDefs;
			return outputBytes;
		}

		/// <summary>
		/// Compresses the bytes if the browser supports it
		/// </summary>
		public override byte[] CompressBytes(CompressionType type, byte[] fileBytes)
		{
            return SimpleCompressor.CompressBytes(type, fileBytes);
		}

	    /// <summary>
	    /// Writes the output of an external request to the stream. Returns true/false if succesful or not.
	    /// </summary>
	    /// <param name="sw"></param>
	    /// <param name="url"></param>
	    /// <param name="type"></param>
	    /// <param name="fileDefs"></param>
	    /// <param name="http"></param>
	    /// <returns></returns>
	    protected virtual void WriteFileToStream(ref StreamWriter sw, string url, ClientDependencyType type, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
		{
			string requestOutput;
            var rVal = TryReadUri(url, out requestOutput, http);
			if (rVal)
			{
				//write the contents of the external request.
				WriteContentToStream(ref sw, MinifyFile(ParseCssFilePaths(requestOutput, type, url, http), type), type, http);
				fileDefs.Add(new CompositeFileDefinition(url, false));
			}
	        return;
		}

        protected virtual void WriteFileToStream(ref StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
		{
			try
			{
				//if it is a file based dependency then read it				
                var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8

				WriteContentToStream(ref sw, MinifyFile(ParseCssFilePaths(fileContents, type, origUrl, http), type), type, http);
				fileDefs.Add(new CompositeFileDefinition(origUrl, true));
			    return;
			}
			catch (Exception ex)
			{
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
			    return;
			}
		}

		private void WriteContentToStream(ref StreamWriter sw, string content, ClientDependencyType type, HttpContextBase context)
		{
			//need to parse
			if (type == ClientDependencyType.Css && ClientDependencySettings.Instance.DefaultFileRegistrationProvider.EnableCompositeFiles)
			{
				while(_importCssRegex.IsMatch(content))
				{
					var match = _importCssRegex.Match(content);
					//write the previous content
					sw.WriteLine(content.Substring(0, match.Index));
					//write import css content
					var filePath = match.Groups[1].Value.Trim('\'', '"');
					try
					{
						var importContent = string.Empty;
						if(filePath.StartsWith("http"))
						{
							importContent = ParseCssFilePaths(DownloadString(filePath), ClientDependencyType.Css, filePath, context);
						}
						else
						{
							importContent = ParseCssFilePaths(File.ReadAllText(context.Server.MapPath(filePath)), ClientDependencyType.Css, filePath, context);
						}
						
						sw.WriteLine(importContent);
					}
					catch
					{
						sw.WriteLine(string.Format("/*Invalid CSS: {0}*/", filePath));
					}

					content = content.Substring(match.Index + match.Length);
				}
				sw.WriteLine(content);
			}
			else
			{
				sw.WriteLine(MinifyFile(content, type));
			}
		}

		private string DownloadString(string url)
		{
			var content = string.Empty;
			try
			{
				var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
				webRequest.UserAgent = "ClientDependency Css Parser";
				var response = webRequest.GetResponse();
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					content = reader.ReadToEnd();
				}
				response.Close();
			}
			catch
			{
				content = string.Format("/*Invalid CSS: {0}*/", url);
			}

			return content;
		}
	}
}
