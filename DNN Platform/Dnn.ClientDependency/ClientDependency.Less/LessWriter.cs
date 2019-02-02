using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Less;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.configuration;

namespace ClientDependency.Less
{
    /// <summary>
    /// A file writer for dotLess
    /// </summary>
    public sealed class LessWriter : IFileWriter
    {

        [SecuritySafeCritical]
        public bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi,
            ClientDependencyType type, string origUrl, HttpContextBase http, IEnumerable<string> allowedDomains)
        {
            try
            {
                //if it is a file based dependency then read it				
                var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8

                //for our custom file reader to work we just need to put the origUrl into the httpcontext items so 
                //we can retreive it in the reader to then figure out the 'real' requested path.
                http.Items["Cdf_LessWriter_origUrl"] = origUrl;
                //get the default less config
                var config = DotlessConfiguration.GetDefaultWeb();
                //set the file reader to our custom file reader
                config.LessSource = typeof(CdfFileReader);
                //disable cache for this engine since we are already caching our own, plus enabling this will cause errors because
                // the import paths aren't resolved properly.
                config.CacheEnabled = false;
                //get the engine based on the custom config with our custom file reader
                var lessEngine = LessWeb.GetEngine(config);

                var output = lessEngine.TransformToCss(fileContents, origUrl);

                DefaultFileWriter.WriteContentToStream(provider, sw, output, type, http, origUrl, allowedDomains);
                
                return true;
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
                return false;
            }
        }
    }
}
