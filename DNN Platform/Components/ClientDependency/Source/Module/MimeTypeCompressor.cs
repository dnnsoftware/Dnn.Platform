using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.Module
{
    public class MimeTypeCompressor
    {
        public MimeTypeCompressor(HttpContextBase ctx)
        {
            Context = ctx;
            MatchedTypes = ClientDependencySettings.Instance
                .ConfigSection
                .CompositeFileElement
                .MimeTypeCompression
                .Cast<MimeTypeCompressionElement>()
                .Where(x => Context.Request.ContentType.ToUpper().Split(';').Contains(x.MimeType.ToUpper()));
        }

        protected HttpContextBase Context;
        protected IEnumerable<MimeTypeCompressionElement> MatchedTypes;

        public void AddCompression()
        {
            HttpRequestBase request = Context.Request;
            HttpResponseBase response = Context.Response;

            //if debug is on, then don't compress
            if (!Context.IsDebuggingEnabled)
            {
                //check if this request should be compressed based on the mime type and path
                var m = GetSupportedPath(); 
                if (IsSupportedMimeType() && m != null)
                {
                    var cType = Context.GetClientCompression();
                    Context.AddCompressionResponseHeader(cType);

                    

                    if (cType == CompressionType.deflate)
                    {
                        response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                    }
                    else if (cType == CompressionType.gzip)
                    {
                        response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                    }

                    

                }
            }
        }

        protected MimeTypeCompressionElement GetSupportedPath()
        {
            //we're not supporting the ASP.Net AJAX calls for compression
            var uRawUrl = Context.Request.RawUrl.ToUpper();
            if (uRawUrl.Contains("WEBRESOURCE.AXD") || uRawUrl.Contains("SCRIPTRESOURCE.AXD"))
                return null;

            foreach (var m in MatchedTypes)
            {
                //if it is only "*" then convert it to proper regex
                var reg = m.FilePath == "*" ? ".*" : m.FilePath;
                var matched = Regex.IsMatch(Context.Request.RawUrl, reg, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (matched) return m;
            }
            return null;
        }

        protected bool IsSupportedMimeType()
        {
            return MatchedTypes.Count() > 0;

        }

    }
}
