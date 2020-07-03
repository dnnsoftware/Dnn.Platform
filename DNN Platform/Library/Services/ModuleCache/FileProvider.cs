// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    public class FileProvider : ModuleCachingProvider
    {
        private const string DataFileExtension = ".data.resources";
        private const string AttribFileExtension = ".attrib.resources";
        private static readonly SharedDictionary<int, string> CacheFolderPath = new SharedDictionary<int, string>(LockingStrategy.ReaderWriter);

        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
                while (varyByParms.MoveNext())
                {
                    string key = varyByParms.Current.Key.ToLowerInvariant();
                    cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                }
            }

            return this.GenerateCacheKeyHash(tabModuleId, cacheKey.ToString());
        }

        public override int GetItemCount(int tabModuleId)
        {
            return GetCachedItemCount(tabModuleId);
        }

        public override byte[] GetModule(int tabModuleId, string cacheKey)
        {
            string cachedModule = GetCachedOutputFileName(tabModuleId, cacheKey);
            if (!File.Exists(cachedModule))
            {
                return null;
            }

            var fInfo = new FileInfo(cachedModule);
            long numBytes = fInfo.Length;
            using (var fStream = new FileStream(cachedModule, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fStream))
            {
                return br.ReadBytes(Convert.ToInt32(numBytes));
            }
        }

        public override void PurgeCache(int portalId)
        {
            this.PurgeCache(GetCacheFolder(portalId));
        }

        public override void PurgeExpiredItems(int portalId)
        {
            try
            {
                var filesNotDeleted = new StringBuilder();
                int i = 0;
                string cacheFolder = GetCacheFolder(portalId);
                if (Directory.Exists(cacheFolder) && IsPathInApplication(cacheFolder))
                {
                    foreach (string File in Directory.GetFiles(cacheFolder, string.Format("*{0}", AttribFileExtension)))
                    {
                        if (this.IsFileExpired(File))
                        {
                            string fileToDelete = File.Replace(AttribFileExtension, DataFileExtension);
                            if (!FileSystemUtils.DeleteFileWithWait(fileToDelete, 100, 200))
                            {
                                filesNotDeleted.Append(string.Format("{0};", fileToDelete));
                            }
                            else
                            {
                                i += 1;
                            }
                        }
                    }
                }

                if (filesNotDeleted.Length > 0)
                {
                    throw new IOException(string.Format("Deleted {0} files, however, some files are locked.  Could not delete the following files: {1}", i, filesNotDeleted));
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }
        }

        public override void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] output)
        {
            try
            {
                string cachedOutputFile = GetCachedOutputFileName(tabModuleId, cacheKey);

                if (File.Exists(cachedOutputFile))
                {
                    FileSystemUtils.DeleteFileWithWait(cachedOutputFile, 100, 200);
                }

                string attribFile = GetAttribFileName(tabModuleId, cacheKey);

                File.WriteAllBytes(cachedOutputFile, output);
                File.WriteAllLines(attribFile, new[] { DateTime.UtcNow.Add(duration).ToString(CultureInfo.InvariantCulture) });
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }
        }

        public override void Remove(int tabModuleId)
        {
            try
            {
                ModuleInfo tabModule = ModuleController.Instance.GetTabModule(tabModuleId);

                int portalId = tabModule.PortalID;
                if (portalId == Null.NullInteger)
                {
                    portalId = PortalSettings.Current.PortalId;
                }

                string cacheFolder = GetCacheFolder(portalId);
                var filesNotDeleted = new StringBuilder();
                int i = 0;
                foreach (string File in Directory.GetFiles(cacheFolder, tabModuleId + "_*.*"))
                {
                    if (!FileSystemUtils.DeleteFileWithWait(File, 100, 200))
                    {
                        filesNotDeleted.Append(File + ";");
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
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }
        }

        private static string GetAttribFileName(int tabModuleId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(), cacheKey, AttribFileExtension);
        }

        private static int GetCachedItemCount(int tabModuleId)
        {
            return Directory.GetFiles(GetCacheFolder(), string.Format("*{0}", DataFileExtension)).Length;
        }

        private static string GetCachedOutputFileName(int tabModuleId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(), cacheKey, DataFileExtension);
        }

        /// <summary>
        /// [jmarino]  2011-06-16 Check for ContainsKey for a write added.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
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

            if (!string.IsNullOrEmpty(homeDirectoryMapPath))
            {
                cacheFolder = string.Concat(homeDirectoryMapPath, "Cache\\Pages\\");
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
            }

            using (var writerLock = CacheFolderPath.GetWriteLock())
            {
                if (!CacheFolderPath.ContainsKey(portalId))
                {
                    CacheFolderPath.Add(portalId, cacheFolder);
                }
            }

            return cacheFolder;
        }

        private static string GetCacheFolder()
        {
            int portalId = PortalController.Instance.GetCurrentPortalSettings().PortalId;
            return GetCacheFolder(portalId);
        }

        private static bool IsPathInApplication(string cacheFolder)
        {
            return cacheFolder.Contains(Globals.ApplicationMapPath);
        }

        private string GenerateCacheKeyHash(int tabModuleId, string cacheKey)
        {
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                hash = sha256.ComputeHash(hash);
                return tabModuleId + "_" + this.ByteArrayToString(hash);
            }
        }

        private bool IsFileExpired(string file)
        {
            StreamReader oRead = null;
            try
            {
                oRead = File.OpenText(file);
                DateTime expires = DateTime.Parse(oRead.ReadLine(), CultureInfo.InvariantCulture);
                if (expires < DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // if check expire time failed, then force to expire the cache.
                return true;
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
            foreach (string File in Directory.GetFiles(folder, "*.resources"))
            {
                if (!FileSystemUtils.DeleteFileWithWait(File, 100, 200))
                {
                    filesNotDeleted.Append(string.Format("{0};", File));
                }
                else
                {
                    i += 1;
                }
            }

            if (filesNotDeleted.Length > 0)
            {
                throw new IOException(string.Format("Deleted {0} files, however, some files are locked.  Could not delete the following files: {1}", i, filesNotDeleted));
            }
        }
    }
}
