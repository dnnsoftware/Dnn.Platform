using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using SassAndCoffee.Core;
using SassAndCoffee.JavaScript;
using SassAndCoffee.JavaScript.CoffeeScript;

namespace ClientDependency.Coffee
{
    /// <summary>
    /// A file writer for CoffeeScript
    /// </summary>
    public sealed class CoffeeWriter : IFileWriter
    {
        public bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi,
            ClientDependencyType type, string origUrl, HttpContextBase http, IEnumerable<string> allowedDomains)
        {
            try
            {
                //if it is a file based dependency then read it				
                var fileContents = File.ReadAllText(fi.FullName, Encoding.UTF8); //read as utf 8

                var output = GetOutput(fileContents);

                DefaultFileWriter.WriteContentToStream(provider, sw, output, type, http, origUrl, allowedDomains);
                
                return true;
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not write file {0} contents to stream. EXCEPTION: {1}", fi.FullName, ex.Message), ex);
                return false;
            }
        }

        public string GetOutput(string input)
        {
            var compiler = new CoffeeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime()));
            return compiler.Compile(input);
        }
    }
}


