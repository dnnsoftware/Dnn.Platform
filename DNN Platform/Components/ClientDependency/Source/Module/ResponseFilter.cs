using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using System.Net;


namespace ClientDependency.Core.Module
{
    /// <summary>
    /// Used as an http response filter to modify the contents of the output html.
    /// This is used to process Rogue Files and MVC responses.
    /// </summary>
    internal class ResponseFilter : Stream
    {

        public ResponseFilter(Stream inputStream, HttpContextBase ctx)
        {
            m_ResponseStream = inputStream;
            m_ResponseHtml = new StringBuilder();
            m_Context = ctx;
            m_MvcFilter = new MvcFilter(m_Context);
            m_RogueFileFilter = new RogueFileFilter(m_Context);
        }

        #region Private members

        private Stream m_ResponseStream;
        private long m_Position;
        private StringBuilder m_ResponseHtml;

        private HttpContextBase m_Context;

        private IFilter m_RogueFileFilter;
        private IFilter m_MvcFilter;

        #endregion

        #region Basic Stream implementation overrides
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }
        #endregion

        #region Stream wrapper implementation
        public override void Close()
        {
            m_ResponseStream.Close();
        }

        public override long Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_ResponseStream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            m_ResponseStream.SetLength(length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_ResponseStream.Read(buffer, offset, count);
        }
        #endregion

        #region Stream implemenation that does stuff

        /// <summary>
        /// Appends the bytes written to our string builder
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            m_ResponseHtml.Append(System.Text.Encoding.Default.GetString(buffer));
        }

        /// <summary>
        /// Before the contents are flushed to the stream, the output is inspected and altered
        /// and then written to the stream.
        /// </summary>
        public override void Flush()
        {
            UpdateOutputHtml();
            m_ResponseStream.Flush();
        }

        #endregion


        /// <summary>
        /// Replaces any rogue script tag's with calls to the compression handler instead 
        /// of just the script.
        /// </summary>
        private void UpdateOutputHtml()
        {
            var output = m_ResponseHtml.ToString();
            
            output = m_RogueFileFilter.UpdateOutputHtml(output);
            output = m_MvcFilter.UpdateOutputHtml(output);
            
            byte[] outputBytes = System.Text.Encoding.Default.GetBytes(output);
            m_ResponseStream.Write(outputBytes, 0, outputBytes.GetLength(0));
        }
    }
}
