#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// FileProvider implements the OutputCachingProvider for file storage.
    /// </summary>
    public class FileProvider : OutputCachingProvider
    {
        public const string DataFileExtension = ".data.resources";
        public const string AttribFileExtension = ".attrib.resources";
        public const string TempFileExtension = ".temp.resources";

        private static readonly SharedDictionary<int, string> CacheFolderPath = new SharedDictionary<int, string>(LockingStrategy.ReaderWriter);

        #region Private Methods

        private static string GetCacheFolder()
        {
            int portalId = PortalController.Instance.GetCurrentPortalSettings().PortalId;
            return GetCacheFolder(portalId);
        }

        private static string GetCacheFolder(int portalId)
        {
            string cacheFolder;

            using (var readerLock = CacheFolderPath.GetReadLock())
            {
                if (CacheFolderPath.TryGetValue(portalId, out cacheFolder))
                {
                    return cacheFolder;
                }
            }

            var portalInfo = PortalController.Instance.GetPortal(portalId);

            string homeDirectoryMapPath = portalInfo.HomeSystemDirectoryMapPath;
            

            if (!(string.IsNullOrEmpty(homeDirectoryMapPath)))
            {
                cacheFolder = string.Concat(homeDirectoryMapPath, "Cache\\Pages\\");
                if (!(Directory.Exists(cacheFolder)))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
            }

            using (var writerLock = CacheFolderPath.GetWriteLock())
            {
                CacheFolderPath.Add(portalId, cacheFolder);
                
            }

            return cacheFolder;
        }

        private bool IsFileExpired(string file)
        {
            StreamReader oRead = null;
            try
            {
                oRead = File.OpenText(file);
                DateTime expires = Convert.ToDateTime(oRead.ReadLine());
                if (expires < DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (oRead != null)
                {
                    oRead.Close();
                }
            }
        }

        private void PurgeCache(string folder)
        {
            var filesNotDeleted = new StringBuilder();
            int i = 0;
            foreach (string file in Directory.GetFiles(folder, "*.resources"))
            {
                if (!(FileSystemUtils.DeleteFileWithWait(file, 100, 200)))
                {
                    filesNotDeleted.Append(file + ";");
                }
                else
                {
                    i += 1;
                }
            }
            if (filesNotDeleted.Length > 0)
            {
                throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
            }
        }

        #endregion

        #region Friend Methods

        internal static string GetAttribFileName(int tabId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(), cacheKey, AttribFileExtension);
        }

        internal static int GetCachedItemCount(int tabId)
        {
            return Directory.GetFiles(GetCacheFolder(), "*" + DataFileExtension).Length;
        }

        internal static string GetCachedOutputFileName(int tabId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(), cacheKey, DataFileExtension);
        }

        internal static string GetTempFileName(int tabId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(), cacheKey, TempFileExtension);
        }

        #endregion

        #region Abstract Method Implementation

        public override int GetItemCount(int tabId)
        {
            return GetCachedItemCount(tabId);
        }

        public override byte[] GetOutput(int tabId, string cacheKey)
        {
            string cachedOutput = GetCachedOutputFileName(tabId, cacheKey);
            if (!(File.Exists(cachedOutput)))
            {
                return null;
            }
            var fInfo = new FileInfo(cachedOutput);
            long numBytes = fInfo.Length;
            using (var fStream = new FileStream(cachedOutput, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fStream))
            {
                return br.ReadBytes(Convert.ToInt32(numBytes));
            }
        }

        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
            return new FileResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        public override void PurgeCache(int portalId)
        {
            string cacheFolder = GetCacheFolder(portalId);
            if (!(string.IsNullOrEmpty(cacheFolder)))
            {
                PurgeCache(cacheFolder);
            }
        }

        public override void PurgeExpiredItems(int portalId)
        {
            var filesNotDeleted = new StringBuilder();
            int i = 0;
            string cacheFolder = GetCacheFolder(portalId);

            if (!(string.IsNullOrEmpty(cacheFolder)))
            {
                foreach (string file in Directory.GetFiles(cacheFolder, "*" + AttribFileExtension))
                {
                    if (IsFileExpired(file))
                    {
                        string fileToDelete = file.Replace(AttribFileExtension, DataFileExtension);
                        if (!(FileSystemUtils.DeleteFileWithWait(fileToDelete, 100, 200)))
                        {
                            filesNotDeleted.Append(fileToDelete + ";");
                        }
                        else
                        {
                            i += 1;
                        }
                    }
                }
                if (filesNotDeleted.Length > 0)
                {
                    throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
                }
            }
        }

        public override void Remove(int tabId)
        {
            Dictionary<int, int> portals = PortalController.GetPortalDictionary();
            if (portals.ContainsKey(tabId) && portals[tabId] > Null.NullInteger)
            {
                var filesNotDeleted = new StringBuilder();
                int i = 0;
                string cacheFolder = GetCacheFolder(portals[tabId]);

                if (!(string.IsNullOrEmpty(cacheFolder)))
                {
                    foreach (string file in Directory.GetFiles(cacheFolder, string.Concat(tabId, "_*.*")))
                    {
                        if (!(FileSystemUtils.DeleteFileWithWait(file, 100, 200)))
                        {
                            filesNotDeleted.Append(string.Concat(file, ";"));
                        }
                        else
                        {
                            i += 1;
                        }
                    }
                    if (filesNotDeleted.Length > 0)
                    {
                        var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};

                        var logDetail = new LogDetailInfo
                        {
                            PropertyName = "FileOutputCacheProvider",
                            PropertyValue =
                                string.Format(
                                    "Deleted {0} files, however, some files are locked.  Could not delete the following files: {1}",
                                    i, filesNotDeleted)
                        };
                        var properties = new LogProperties {logDetail};
                        log.LogProperties = properties;

                        LogController.Instance.AddLog(log);
                    }
                }
            }
        }

        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
            string attribFile = GetAttribFileName(tabId, cacheKey);
            string cachedOutputFile = GetCachedOutputFileName(tabId, cacheKey);

            try
            {
                if (File.Exists(cachedOutputFile))
                {
                    FileSystemUtils.DeleteFileWithWait(cachedOutputFile, 100, 200);
                }

                using (var captureStream = new FileStream(cachedOutputFile, FileMode.CreateNew, FileAccess.Write))
                {
                    captureStream.Write(output, 0, output.Length);
                    captureStream.Close();
                }

                using (var oWrite = File.CreateText(attribFile))
                {
                    oWrite.WriteLine(DateTime.UtcNow.Add(duration).ToString());
                    oWrite.Close();
                }
            }
            catch (Exception ex)
            {
                // TODO: Need to implement multi-threading.  
                // The current code is not thread safe and threw error if two threads tried creating cache file
                // A thread could create a file between the time another thread deleted it and tried to create new cache file.
                // This would result in a system.IO.IOException.  Also, there was no error handling in place so the 
                // Error would bubble up to the user and provide details on the file structure of the site.
                Exceptions.Exceptions.LogException(ex);
            }
        }

        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
            bool foundFile;
            try
            {
                string attribFile = GetAttribFileName(tabId, cacheKey);
                string captureFile = GetCachedOutputFileName(tabId, cacheKey);
                StreamReader oRead = File.OpenText(attribFile);
                DateTime expires = Convert.ToDateTime(oRead.ReadLine());
                oRead.Close();
                if (expires < DateTime.UtcNow)
                {
                    FileSystemUtils.DeleteFileWithWait(attribFile, 100, 200);
                    FileSystemUtils.DeleteFileWithWait(captureFile, 100, 200);
                    return false;
                }

				context.Response.WriteFile(captureFile);

                foundFile = true;
            }
            catch (FileNotFoundException)
            {
                foundFile = false;
            }
            return foundFile;
        }

        #endregion
    }
}