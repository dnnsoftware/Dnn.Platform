using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    internal interface IFileReader
    {
        string ReadFile(string filePath);
    }

    internal class VirtualPathFileReader : IFileReader
    {
        public string ReadFile(string filePath)
        {
            var file = HostingEnvironment.VirtualPathProvider.GetFile(filePath);
            if (file == null) return string.Empty;
            string output;
            using (var reader = new StreamReader(file.Open()))
            {
                output = reader.ReadToEnd();
                reader.Close();
            }
            return output;
        }
    }

    internal class PhysicalFileReader : IFileReader
    {
        private readonly HttpContextBase _httpContext;

        public PhysicalFileReader(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public string ReadFile(string filePath)
        {
            var physicalPath = _httpContext.Server.MapPath(filePath);
            if (physicalPath == null) return "";
            if (!File.Exists(physicalPath)) return "";
            try
            {
                return File.ReadAllText(physicalPath);
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error(string.Format("Could not read file {0}. EXCEPTION: {1}", physicalPath, ex.Message), ex);
                return "";
            }
        }
    }
}
