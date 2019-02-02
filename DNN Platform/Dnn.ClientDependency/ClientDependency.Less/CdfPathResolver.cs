using System.IO;
using System.Security;
using System.Web;
using dotless.Core.Input;

namespace ClientDependency.Less
{
    [SecurityCritical]
    public sealed class CdfPathResolver : IPathResolver
    {
        private readonly HttpContextBase _http;
        private readonly string _origUrl;
        [SecurityCritical]
        private readonly AspRelativePathResolver _inner = new AspRelativePathResolver();
        private readonly string _currentFolder;

        public CdfPathResolver(HttpContextBase http, string origUrl)
        {
            _http = http;
            _origUrl = origUrl;

            _currentFolder = Path.GetDirectoryName(_http.Server.MapPath(_origUrl));
        }

        [SecurityCritical]
        public string GetFullPath(string path)
        {
            if (path.StartsWith("/"))
            {
                //its an absolute path, combine it to the app physical path
                return Path.Combine(
                    _http.Request.PhysicalApplicationPath,
                    path.Replace('/', Path.DirectorySeparatorChar).TrimStart('\\'));
            }
            
            if (path.StartsWith("~/"))
            {
                //its a virtual path, so we can just map path it
                return _http.Server.MapPath(path);
            }

            //it's a relative path so add it to the current request's path
            path = path.Replace('/', Path.DirectorySeparatorChar);            
            return Path.Combine(_currentFolder, path);
        }
    }
}