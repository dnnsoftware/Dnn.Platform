#region Copyright
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.IO;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.OutputCache.Providers
{
    // helper class to capture the response into memory

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