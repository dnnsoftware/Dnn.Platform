using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace DotNetNuke.Services.GeneratedImage
{
    public class IPCount
    {
        private const string TempFileExtension = ".tmp";
        private const string CacheAppRelativePath = @"~\App_Data\_ipcount\";
        private static string _cachePath;
        private static readonly object PurgeQueuedLock = new object();
        private static bool _purgeQueued;
        private static TimeSpan _purgeInterval;
        private static readonly object FileLock = new object();

        public static string CachePath
        {
            get { return _cachePath; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
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
                    throw new ArgumentNullException("value");
                }
                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _purgeInterval = value;
            }
        }

        public static int MaxCount { get; set; }

        private static DateTime LastPurge
        {
            get
            {
                DateTime lastPurge = DateTime.Now;
                if (File.Exists(CachePath + "_lastpurge"))
                {
                    FileInfo fi = new FileInfo(CachePath + "_lastpurge");
                    lastPurge = fi.LastWriteTime;
                }
                else
                {
                    File.WriteAllText(CachePath + "_lastpurge", "");
                }
                return lastPurge;
            }
            
            set
            {
                File.WriteAllText(CachePath + "_lastpurge", "");
            }
        }

        static IPCount()
        {
            PurgeInterval = new TimeSpan(0, 10, 0);
            MaxCount = 500;
            CachePath = HostingEnvironment.MapPath(CacheAppRelativePath);
        }

        internal IPCount()
        {
            if (CachePath != null && !Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }
        }


        public static bool CheckIp(string ipAddress)
        {

            DateTime now = DateTime.Now;
            if (!_purgeQueued && now.Subtract(LastPurge) > PurgeInterval)
            {
                lock (PurgeQueuedLock)
                {
                    if (!_purgeQueued)
                    {
                        _purgeQueued = true;

                        var files = new DirectoryInfo(CachePath).GetFiles();
                        DateTime threshold = DateTime.Now.Subtract(PurgeInterval);
                        List<FileInfo> toTryDeleteAgain = new List<FileInfo>();
                        foreach (var fileinfo in files)
                        {
                            if (fileinfo.Name.ToLower() != "_lastpurge" && fileinfo.LastWriteTime < threshold)
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

            string path = BuildFilePath(ipAddress);
            int count = 1;
            lock (FileLock)
            {
                if (File.Exists(path))
                {
                    string strCount = File.ReadAllText(path);
                    if (Int32.TryParse(strCount, out count))
                    {
                        if (count > MaxCount)
                            return false;

                        count++;
                    }
                }
                File.WriteAllText(path, count.ToString());
                return true;
            }
        }

        private static string BuildFilePath(string ipAddress)
        {
            return CachePath + ipAddress + TempFileExtension;
        }

                /// <summary>
        /// method to get Client ip address
        /// </summary>
        /// <returns>IP Address of visitor</returns>
        public static string GetVisitorIPAddress(HttpContextBase context)
        {
            string visitorIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = context.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = context.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                visitorIPAddress = string.Empty;
            }

            if (string.IsNullOrEmpty(visitorIPAddress))
            {
                //This is for Local(LAN) Connected ID Address
                string stringHostName = Dns.GetHostName();
                //Get Ip Host Entry
                IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                //Get Ip Address From The Ip Host Entry Address List
                IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                try
                {
                    visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                }
                catch
                {
                    try
                    {
                        visitorIPAddress = arrIpAddress[0].ToString();
                    }
                    catch
                    {
                        try
                        {
                            arrIpAddress = Dns.GetHostAddresses(stringHostName);
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            visitorIPAddress = "127.0.0.1";
                        }
                    }
                }
            }
            return visitorIPAddress;
        }
    }
}
