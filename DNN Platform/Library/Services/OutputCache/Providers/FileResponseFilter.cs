#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// FileResponseFilter implements the OutputCacheRepsonseFilter to capture the response into files.
    /// </summary>
    public class FileResponseFilter : OutputCacheResponseFilter
    {
        //Private _content As StringBuilder
        private DateTime _cacheExpiration;

        internal FileResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration) : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
            if (maxVaryByCount > -1 && Services.OutputCache.Providers.FileProvider.GetCachedItemCount(itemId) >= maxVaryByCount)
            {
                HasErrored = true;
                return;
            }

            CachedOutputTempFileName = Services.OutputCache.Providers.FileProvider.GetTempFileName(itemId, cacheKey);
            CachedOutputFileName = Services.OutputCache.Providers.FileProvider.GetCachedOutputFileName(itemId, cacheKey);
            CachedOutputAttribFileName = Services.OutputCache.Providers.FileProvider.GetAttribFileName(itemId, cacheKey);
            if (File.Exists(CachedOutputTempFileName))
            {
                bool fileDeleted = FileSystemUtils.DeleteFileWithWait(CachedOutputTempFileName, 100, 200);
                if (fileDeleted == false)
                {
                    HasErrored = true;
                }
            }
            if (HasErrored == false)
            {
                try
                {
                    CaptureStream = new FileStream(CachedOutputTempFileName, FileMode.CreateNew, FileAccess.Write);
                }
                catch (Exception)
                {
                    HasErrored = true;
                    throw;
                }

                _cacheExpiration = DateTime.UtcNow.Add(cacheDuration);
                HasErrored = false;
            }
        }

        public string CachedOutputTempFileName { get; set; }

        public string CachedOutputFileName { get; set; }

        public string CachedOutputAttribFileName { get; set; }

        public DateTime CacheExpiration
        {
            get
            {
                return _cacheExpiration;
            }
            set
            {
                _cacheExpiration = value;
            }
        }

        public override byte[] StopFiltering(int itemId, bool deleteData)
        {
            if (HasErrored)
            {
                return null;
            }

            if ((CaptureStream) != null)
            {
                CaptureStream.Close();

                if (File.Exists(CachedOutputFileName))
                {
                    FileSystemUtils.DeleteFileWithWait(CachedOutputFileName, 100, 200);
                }

                File.Move(CachedOutputTempFileName, CachedOutputFileName);

                StreamWriter oWrite = File.CreateText(CachedOutputAttribFileName);
                oWrite.WriteLine(_cacheExpiration.ToString());
                oWrite.Close();
            }
            if (deleteData)
            {
                FileSystemUtils.DeleteFileWithWait(CachedOutputFileName, 100, 200);
                FileSystemUtils.DeleteFileWithWait(CachedOutputAttribFileName, 100, 200);
            }

            return null;
        }
    }
}