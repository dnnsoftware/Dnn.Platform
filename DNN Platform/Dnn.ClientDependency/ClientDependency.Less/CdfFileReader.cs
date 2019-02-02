using System.Security;
using System.Web;
using dotless.Core.Input;

namespace ClientDependency.Less
{
    [SecurityCritical]
    public class CdfFileReader : dotless.Core.Input.IFileReader
    {
        public CdfFileReader()
        {
            _innerReader = new FileReader(
                new CdfPathResolver(new HttpContextWrapper(HttpContext.Current), (string)HttpContext.Current.Items["Cdf_LessWriter_origUrl"]));
        }

        private readonly FileReader _innerReader;

        [SecurityCritical]
        public byte[] GetBinaryFileContents(string fileName)
        {
            return _innerReader.GetBinaryFileContents(fileName);
        }

        [SecurityCritical]
        public string GetFileContents(string fileName)
        {
            return _innerReader.GetFileContents(fileName);
        }

        [SecurityCritical]
        public bool DoesFileExist(string fileName)
        {
            return _innerReader.DoesFileExist(fileName);
        }

        public bool UseCacheDependencies
        {
            [SecurityCritical]
            get { return false; }
        }
    }
}