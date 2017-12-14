using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	    public override FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server)
		{
            //don't save the file if composite files are disabled.
            if (!PersistCompositeFiles)
                return null;

            if (!CompositeFilePath.Exists)
                CompositeFilePath.Create();
			
            var fi = new FileInfo(
                Path.Combine(CompositeFilePath.FullName,
					ClientDependencySettings.Instance.Version + "_"
                        + Guid.NewGuid().ToString("N") + ".cd" + type.ToString().Substring(0, 1).ToUpper()));
			
            if (fi.Exists)
				fi.Delete();

            //*** DNN related change ***  begin
            using (var fs = fi.Create())
            {
                fs.Write(fileContents, 0, fileContents.Length);
                fs.Close();
            }
            //*** DNN related change ***  end
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
            //*** DNN related change *** begin
            using (var ms = new MemoryStream(5000))
            using (var sw = new StreamWriter(ms, Encoding.UTF8))
            {
                var fDefs = filePaths.Select(s => WritePathToStream(type, s, context, sw)).Where(def => def != null).ToList();

                sw.Flush();
                byte[] outputBytes = ms.ToArray();
                sw.Close();
                ms.Close();
                fileDefs = fDefs;
                return outputBytes;
            }
            //*** DNN related change *** end
		}

		/// <summary>
		/// Compresses the bytes if the browser supports it
		/// </summary>
		public override byte[] CompressBytes(CompressionType type, byte[] fileBytes)
		{
            return SimpleCompressor.CompressBytes(type, fileBytes);
		}
        
	    /// <summary>
	    /// Writes the output of an external request to the stream
	    /// </summary>
	    /// <param name="sw"></param>
	    /// <param name="url"></param>
	    /// <param name="type"></param>
	    /// <param name="fileDefs"></param>
	    /// <param name="http"></param>
	    /// <returns></returns>
	    [Obsolete("Use the equivalent method without the 'ref' parameters")]
        [EditorBrowsable(EditorBrowsableState.Never)]
	    protected virtual void WriteFileToStream(ref StreamWriter sw, string url, ClientDependencyType type, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
	    {
	        var def = WriteFileToStream(sw, url, type, http);
            if (def != null)
            {
                fileDefs.Add(def);
            }
	    }
        
        /// <summary>
        ///  Writes the output of a local file to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="fileDefs"></param>
        /// <param name="http"></param>
        [Obsolete("Use the equivalent method without the 'ref' parameters")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void WriteFileToStream(ref StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, ref List<CompositeFileDefinition> fileDefs, HttpContextBase http)
        {
            var def = WriteFileToStream(sw, fi, type, origUrl, http);
            if (def != null)
            {
                fileDefs.Add(def);
            }
        }
        
	    
	}
}
