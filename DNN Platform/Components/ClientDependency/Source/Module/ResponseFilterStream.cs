using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Text.RegularExpressions;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using System.Net;


namespace ClientDependency.Core.Module
{
    /// <summary>
    /// A semi-generic Stream implementation for Response.Filter with
    /// an event interface for handling Content transformations via
    /// Stream or String.    
    /// <remarks>
    /// Use with care for large output as this implementation copies
    /// the output into a memory stream and so increases memory usage.
    /// </remarks>
    /// </summary>    
    public class 
        ResponseFilterStream : Stream
    {
        /// <summary>
        /// The original stream
        /// </summary>
        readonly Stream _stream;

        private readonly HttpContextBase _http;

        /// <summary>
        /// Current position in the original stream
        /// </summary>
        long _position;

        /// <summary>
        /// Stream that original content is read into
        /// and then passed to TransformStream function
        /// </summary>
        MemoryStream _cacheStream = new MemoryStream(5000);

        /// <summary>
        /// Internal pointer that that keeps track of the size
        /// of the cacheStream
        /// </summary>
        int _cachePointer = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseStream"></param>
        /// <param name="http"></param>
        public ResponseFilterStream(Stream responseStream, HttpContextBase http)
        {
            _stream = responseStream;
            _http = http;
        }


        /// <summary>
        /// Determines whether the stream is captured
        /// </summary>
        private bool IsCaptured
        {
            get
            {

                if (CaptureStream != null || CaptureString != null ||
                    TransformStream != null || TransformString != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Determines whether the Write method is outputting data immediately
        /// or delaying output until Flush() is fired.
        /// </summary>
        private bool IsOutputDelayed
        {
            get
            {
                if (TransformStream != null || TransformString != null)
                    return true;

                return false;
            }
        }


        /// <summary>
        /// Event that captures Response output and makes it available
        /// as a MemoryStream instance. Output is captured but won't 
        /// affect Response output.
        /// </summary>
        public event Action<MemoryStream> CaptureStream;

        /// <summary>
        /// Event that captures Response output and makes it available
        /// as a string. Output is captured but won't affect Response output.
        /// </summary>
        public event Action<string> CaptureString;



        /// <summary>
        /// Event that allows you transform the stream as each chunk of
        /// the output is written in the Write() operation of the stream.
        /// This means that that it's possible/likely that the input 
        /// buffer will not contain the full response output but only
        /// one of potentially many chunks.
        /// 
        /// This event is called as part of the filter stream's Write() 
        /// operation.
        /// </summary>
        public event Func<byte[], byte[]> TransformWrite;


        /// <summary>
        /// Event that allows you to transform the response stream as
        /// each chunk of bytep[] output is written during the stream's write
        /// operation. This means it's possibly/likely that the string
        /// passed to the handler only contains a portion of the full
        /// output. Typical buffer chunks are around 16k a piece.
        /// 
        /// This event is called as part of the stream's Write operation.
        /// </summary>
        public event Func<string, string> TransformWriteString;

        /// <summary>
        /// This event allows capturing and transformation of the entire 
        /// output stream by caching all write operations and delaying final
        /// response output until Flush() is called on the stream.
        /// </summary>
        public event Func<MemoryStream, MemoryStream> TransformStream;

        /// <summary>
        /// Event that can be hooked up to handle Response.Filter
        /// Transformation. Passed a string that you can modify and
        /// return back as a return value. The modified content
        /// will become the final output.
        /// </summary>
        public event Func<string, string> TransformString;


        protected virtual void OnCaptureStream(MemoryStream ms)
        {
            if (CaptureStream != null)
                CaptureStream(ms);
        }


        private void OnCaptureStringInternal(MemoryStream ms)
        {
            if (CaptureString != null)
            {
                string content = _http.Response.ContentEncoding.GetString(ms.ToArray());
                OnCaptureString(content);
            }
        }

        protected virtual void OnCaptureString(string output)
        {
            if (CaptureString != null)
                CaptureString(output);
        }

        protected virtual byte[] OnTransformWrite(byte[] buffer)
        {
            if (TransformWrite != null)
                return TransformWrite(buffer);
            return buffer;
        }

        private byte[] OnTransformWriteStringInternal(byte[] buffer)
        {
            Encoding encoding = _http.Response.ContentEncoding;
            string output = OnTransformWriteString(encoding.GetString(buffer));
            return encoding.GetBytes(output);
        }

        private string OnTransformWriteString(string value)
        {
            if (TransformWriteString != null)
                return TransformWriteString(value);
            return value;
        }


        protected virtual MemoryStream OnTransformCompleteStream(MemoryStream ms)
        {
            if (TransformStream != null)
                return TransformStream(ms);

            return ms;
        }




        /// <summary>
        /// Allows transforming of strings
        /// 
        /// Note this handler is internal and not meant to be overridden
        /// as the TransformString Event has to be hooked up in order
        /// for this handler to even fire to avoid the overhead of string
        /// conversion on every pass through.
        /// </summary>
        /// <param name="responseText"></param>
        /// <returns></returns>
        private string OnTransformCompleteString(string responseText)
        {
            if (TransformString != null)
                TransformString(responseText);

            return responseText;
        }

        /// <summary>
        /// Wrapper method form OnTransformString that handles
        /// stream to string and vice versa conversions
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        internal MemoryStream OnTransformCompleteStringInternal(MemoryStream ms)
        {
            if (TransformString == null)
                return ms;

            //string content = ms.GetAsString();
            string content = _http.Response.ContentEncoding.GetString(ms.ToArray());

            content = TransformString(content);
            byte[] buffer = _http.Response.ContentEncoding.GetBytes(content);
            ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            //ms.WriteString(content);

            return ms;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Length
        {
            get { return 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return _stream.Seek(offset, direction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public override void SetLength(long length)
        {
            _stream.SetLength(length);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            _stream.Close();
        }

        /// <summary>
        /// Override flush by writing out the cached stream data
        /// </summary>
        public override void Flush()
        {

            if (IsCaptured && _cacheStream.Length > 0)
            {
                // Check for transform implementations
                _cacheStream = OnTransformCompleteStream(_cacheStream);
                _cacheStream = OnTransformCompleteStringInternal(_cacheStream);

                OnCaptureStream(_cacheStream);
                OnCaptureStringInternal(_cacheStream);

                // write the stream back out if output was delayed
                if (IsOutputDelayed)
                    _stream.Write(_cacheStream.ToArray(), 0, (int)_cacheStream.Length);

                // Clear the cache once we've written it out
                _cacheStream.SetLength(0);
            }

            // default flush behavior
            _stream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }


        /// <summary>
        /// Overriden to capture output written by ASP.NET and captured
        /// into a cached stream that is written out later when Flush()
        /// is called.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (IsCaptured)
            {
                // copy to holding buffer only - we'll write out later
                _cacheStream.Write(buffer, 0, count);
                _cachePointer += count;
            }

            // just transform this buffer
            if (TransformWrite != null)
                buffer = OnTransformWrite(buffer);
            if (TransformWriteString != null)
                buffer = OnTransformWriteStringInternal(buffer);

            if (!IsOutputDelayed)
                _stream.Write(buffer, offset, count);

        }

    }

}
