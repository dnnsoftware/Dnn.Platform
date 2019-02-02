using System;
using System.IO;
using System.Text;
using System.Web;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// Replaces the placeholders in the view
    /// </summary>
    public class PlaceholderReplacer : IDisposable
    {
        private readonly TextWriter _originalWriter;
        private readonly HttpContextBase _httpContext;
        private readonly StringWriter _writer;

        public PlaceholderReplacer(TextWriter originalWriter, HttpContextBase httpContext)
        {
            _originalWriter = originalWriter;
            _httpContext = httpContext;
            _writer = new StringWriter(new StringBuilder());
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

        private void PerformReplacements()
        {
            var output = _writer.ToString();

            //do replacements
            var replaced = DependencyRenderer.GetInstance(_httpContext).ParseHtmlPlaceholders(output);

            //write to original
            _originalWriter.Write(replaced);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Does the replacements and disposes local resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            
            PerformReplacements();
            _writer.Dispose();
        }
    }
}