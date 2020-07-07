// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Common.Utils;
    using DotNetNuke.Services.UserRequest;

    public class IPCount
    {
        private const string TempFileExtension = ".tmp";
        private const string CacheAppRelativePath = @"~\App_Data\_ipcount\";
        private static readonly object PurgeQueuedLock = new object();
        private static readonly object FileLock = new object();
        private static string _cachePath;
        private static bool _purgeQueued;
        private static TimeSpan _purgeInterval;

        static IPCount()
        {
            PurgeInterval = new TimeSpan(0, 10, 0);
            MaxCount = 500;
            CachePath = HostingEnvironment.MapPath(CacheAppRelativePath);
        }

        public static string CachePath
        {
            get { return _cachePath; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }

                _cachePath = value;
            }
        }

        public static TimeSpan PurgeInterval
        {
            get
            {
                return _purgeInterval;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _purgeInterval = value;
            }
        }

        public static int MaxCount { get; set; }

        private static DateTime LastPurge
        {
            get
            {
                var lastPurge = DateTime.Now;
                if (File.Exists(CachePath + "_lastpurge"))
                {
                    var fi = new FileInfo(CachePath + "_lastpurge");
                    lastPurge = fi.LastWriteTime;
                }
                else
                {
                    File.WriteAllText(CachePath + "_lastpurge", string.Empty);
                }

                return lastPurge;
            }

            set
            {
                File.WriteAllText(CachePath + "_lastpurge", string.Empty);
            }
        }

        public static bool CheckIp(string ipAddress)
        {
            var now = DateTime.Now;
            if (!_purgeQueued && now.Subtract(LastPurge) > PurgeInterval)
            {
                lock (PurgeQueuedLock)
                {
                    if (!_purgeQueued)
                    {
                        _purgeQueued = true;

                        var files = new DirectoryInfo(CachePath).GetFiles();
                        var threshold = DateTime.Now.Subtract(PurgeInterval);
                        var toTryDeleteAgain = new List<FileInfo>();
                        foreach (var fileinfo in files)
                        {
                            if (fileinfo.Name.ToLowerInvariant() != "_lastpurge" && fileinfo.LastWriteTime < threshold)
                            {
                                try
                                {
                                    fileinfo.Delete();
                                }
                                catch (Exception)
                                {
                                    toTryDeleteAgain.Add(fileinfo);
                                }
                            }
                        }

                        Thread.Sleep(0);
                        foreach (var fileinfo in toTryDeleteAgain)
                        {
                            try
                            {
                                fileinfo.Delete();
                            }
                            catch (Exception)
                            {
                                // do nothing at this point, try to delete file during next purge
                            }
                        }

                        LastPurge = DateTime.Now;

                        _purgeQueued = false;
                    }
                }
            }

            var path = BuildFilePath(ipAddress);
            var count = 1;
            lock (FileLock)
            {
                if (File.Exists(path))
                {
                    var strCount = File.ReadAllText(path);
                    if (int.TryParse(strCount, out count))
                    {
                        if (count > MaxCount)
                        {
                            return false;
                        }

                        count++;
                    }
                }

                File.WriteAllText(path, count.ToString());
                return true;
            }
        }

        /// <summary>
        /// method to get Client ip address.
        /// </summary>
        /// <returns>IP Address of visitor.</returns>
        [Obsolete("Deprecated in 9.2.0. Use UserRequestIPAddressController.Instance.GetUserRequestIPAddress. Scheduled removal in v11.0.0.")]
        public static string GetVisitorIPAddress(HttpContextBase context)
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(context.Request);
        }

        private static string BuildFilePath(string ipAddress)
        {
            // it takes only the IP address without PORT for the file name
            var fileName = ipAddress.Split(':')[0];
            return CachePath + fileName + TempFileExtension;
        }
    }
}
