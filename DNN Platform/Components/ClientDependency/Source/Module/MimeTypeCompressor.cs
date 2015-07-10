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
            if (Context == null) return;

            //if debug is on, then don't compress
            if (!Context.IsDebuggingEnabled)
            {
                if (Context.Response == null) return;
                if (Context.Response.Filter == null) return;

                //if the current filter is not the default ASP.Net filter, then we will not continue.
                var filterType = Context.Response.Filter.GetType();
                //the default is normally: System.Web.HttpResponseStreamFilterSink
                //however that is internal, we'll just assume that any filter that is in the namespace
                // System.Web is the default and we can continue.
                if (filterType.Namespace != null && filterType.Namespace.StartsWith("System.Web"))
                {
                    //check if this request should be compressed based on the mime type and path
                    var m = GetSupportedPath();
                    if (IsSupportedMimeType() && m != null)
                    {
                        PerformCompression(Context);
                    }    
                }
            }
        }

        internal static void PerformCompression(HttpContextBase context)
        {
            var cType = context.GetClientCompression();
            context.AddCompressionResponseHeader(cType);

            if (cType == CompressionType.deflate)
            {
                context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
            }
            else if (cType == CompressionType.gzip)
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
            }
        }

        protected MimeTypeCompressionElement GetSupportedPath()
        {
            //we're not supporting the ASP.Net AJAX calls for compression
            var rawUrl = Context.GetRawUrlSafe();

            if (string.IsNullOrWhiteSpace(rawUrl)) return null;

            var uRawUrl = rawUrl.ToUpper();
            if (uRawUrl.Contains("WEBRESOURCE.AXD") || uRawUrl.Contains("SCRIPTRESOURCE.AXD"))
                return null;

            foreach (var m in MatchedTypes)
            {
                //if it is only "*" then convert it to proper regex
                var reg = m.FilePath == "*" ? ".*" : m.FilePath;
                var matched = Regex.IsMatch(rawUrl, reg, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
