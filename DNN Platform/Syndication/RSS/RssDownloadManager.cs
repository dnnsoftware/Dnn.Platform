// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Instrumentation;

    /// <summary>
    ///   Helper class that provides memory and disk caching of the downloaded feeds.
    /// </summary>
    internal class RssDownloadManager
    {
        private const string RSS_Dir = "/RSS/";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssDownloadManager));
        private static readonly RssDownloadManager _theManager = new RssDownloadManager();

        private readonly Dictionary<string, RssChannelDom> _cache;
        private readonly int _defaultTtlMinutes;
        private readonly string _directoryOnDisk;

        private RssDownloadManager()
        {
            // create in-memory cache
            this._cache = new Dictionary<string, RssChannelDom>();

            this._defaultTtlMinutes = 2;

            // prepare disk directory
            this._directoryOnDisk = PrepareTempDir();
        }

        public static RssChannelDom GetChannel(string url)
        {
            return _theManager.GetChannelDom(url);
        }

        private static int GetTtlFromString(string ttlString, int defaultTtlMinutes)
        {
            if (!string.IsNullOrEmpty(ttlString))
            {
                int ttlMinutes;
                if (int.TryParse(ttlString, out ttlMinutes))
                {
                    if (ttlMinutes >= 0)
                    {
                        return ttlMinutes;
                    }
                }
            }

            return defaultTtlMinutes;
        }

        private static string PrepareTempDir()
        {
            string tempDir = null;

            try
            {
                string d = HttpContext.Current.Server.MapPath(Settings.CacheRoot + RSS_Dir);

                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }

                tempDir = d;
            }
            catch
            {
                // don't cache on disk if can't do it
            }

            return tempDir;
        }

        private static string GetTempFileNamePrefixFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return string.Format("{0}_{1:x8}", uri.Host.Replace('.', '_'), uri.AbsolutePath.GetHashCode());
            }
            catch
            {
                return "rss";
            }
        }

        private RssChannelDom DownloadChannelDom(string url)
        {
            // look for disk cache first
            RssChannelDom dom = this.TryLoadFromDisk(url);

            if (dom != null)
            {
                return dom;
            }

            // download the feed
            byte[] feed = new WebClient().DownloadData(url);

            // parse it as XML
            var doc = new XmlDocument { XmlResolver = null };
            doc.Load(new MemoryStream(feed));

            // parse into DOM
            dom = RssXmlHelper.ParseChannelXml(doc);

            // set expiry
            string ttlString = null;
            dom.Channel.TryGetValue("ttl", out ttlString);
            int ttlMinutes = GetTtlFromString(ttlString, this._defaultTtlMinutes);
            DateTime utcExpiry = DateTime.UtcNow.AddMinutes(ttlMinutes);
            dom.SetExpiry(utcExpiry);

            // save to disk
            this.TrySaveToDisk(doc, url, utcExpiry);

            return dom;
        }

        private RssChannelDom TryLoadFromDisk(string url)
        {
            if (this._directoryOnDisk == null)
            {
                return null; // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(url) + "_*.rss.resources";
            string[] files = Directory.GetFiles(this._directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

            foreach (string rssFilename in files)
            {
                XmlDocument rssDoc = null;
                bool isRssFileValid = false;
                DateTime utcExpiryFromRssFile = DateTime.MinValue;
                string urlFromRssFile = null;

                try
                {
                    rssDoc = new XmlDocument { XmlResolver = null };
                    rssDoc.Load(rssFilename);

                    // look for special XML comment (before the root tag)'
                    // containing expiration and url
                    var comment = rssDoc.DocumentElement.PreviousSibling as XmlComment;

                    if (comment != null)
                    {
                        string c = comment.Value;
                        int i = c.IndexOf('@');
                        long expiry;

                        if (long.TryParse(c.Substring(0, i), out expiry))
                        {
                            utcExpiryFromRssFile = DateTime.FromBinary(expiry);
                            urlFromRssFile = c.Substring(i + 1);
                            isRssFileValid = true;
                        }
                    }
                }
                catch
                {
                    // error processing one file shouldn't stop processing other files
                }

                // remove invalid or expired file
                if (!isRssFileValid || utcExpiryFromRssFile < DateTime.UtcNow)
                {
                    try
                    {
                        File.Delete(rssFilename);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    // try next file
                    continue;
                }

                // match url
                if (urlFromRssFile == url)
                {
                    // found a good one - create DOM and set expiry (as found on disk)
                    RssChannelDom dom = RssXmlHelper.ParseChannelXml(rssDoc);
                    dom.SetExpiry(utcExpiryFromRssFile);
                    return dom;
                }
            }

            // not found
            return null;
        }

        private void TrySaveToDisk(XmlDocument doc, string url, DateTime utcExpiry)
        {
            if (this._directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment(string.Format("{0}@{1}", utcExpiry.ToBinary(), url)), doc.DocumentElement);

            string fileName = string.Format("{0}_{1:x8}.rss.resources", GetTempFileNamePrefixFromUrl(url), Guid.NewGuid().ToString().GetHashCode());

            try
            {
                doc.Save(Path.Combine(this._directoryOnDisk, fileName));
            }
            catch
            {
                // can't save to disk - not a problem
            }
        }

        private RssChannelDom GetChannelDom(string url)
        {
            RssChannelDom dom = null;

            lock (this._cache)
            {
                if (this._cache.TryGetValue(url, out dom))
                {
                    if (DateTime.UtcNow > dom.UtcExpiry)
                    {
                        this._cache.Remove(url);
                        dom = null;
                    }
                }
            }

            if (dom == null)
            {
                dom = this.DownloadChannelDom(url);

                lock (this._cache)
                {
                    this._cache[url] = dom;
                }
            }

            return dom;
        }
    }
}
