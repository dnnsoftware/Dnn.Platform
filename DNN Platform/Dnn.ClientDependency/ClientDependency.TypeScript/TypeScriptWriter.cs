using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using SassAndCoffee.Core;
using SassAndCoffee.JavaScript;

namespace ClientDependency.TypeScript
{
    public sealed class TypeScriptWriter : IFileWriter
    {
        public bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi,
            ClientDependencyType type, string origUrl, HttpContextBase http, IEnumerable<string> allowedDomains)
        {
            //if it is a file based dependency then read it				
            var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8

            var engine = GetEngine();

            try
            {
                var output = CompileTypeScript(engine, fileContents);
                DefaultFileWriter.WriteContentToStream(provider, sw, output, type, http, origUrl, allowedDomains);
                return true;
            }
            catch (System.Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
                return false;
            }            
        }

        /// <summary>
        /// Gets the JS engine used to compile the script
        /// </summary>
        /// <returns></returns>
        internal IJavaScriptRuntime GetEngine()
        {
            var jsRuntime = new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime());
            var instance = jsRuntime.GetInstance();
            instance.Initialize();
            instance.LoadLibrary(JsLibs.typescript_min);
            instance.LoadLibrary(JsLibs.compiler);
            return instance;
        }

        /// <summary>
        /// Compiles the TypeScript
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="typeScript"></param>
        /// <returns></returns>
        internal string CompileTypeScript(IJavaScriptRuntime engine, string typeScript)
        {          
            using (engine)
            {
                var result = engine.ExecuteFunction<string>("compileTsSource", typeScript, JsLibs.lib_d);
                if (TypeScriptCompilationErrorParser.HasErrors(result))
                {
                    throw new TypeScriptCompilationException(TypeScriptCompilationErrorParser.Parse(result).ToArray());
                }
                return result;    
            }
        }
    }
}
