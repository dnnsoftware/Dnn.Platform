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
    internal class OpmlDownloadManager
    {
        private const string OPML_Dir = "/OPML/";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OpmlDownloadManager));
        private static readonly OpmlDownloadManager _theManager = new OpmlDownloadManager();

        private readonly Dictionary<string, Opml> _cache;
        private readonly int _defaultTtlMinutes;
        private readonly string _directoryOnDisk;

        private OpmlDownloadManager()
        {
            // create in-memory cache
            this._cache = new Dictionary<string, Opml>();

            this._defaultTtlMinutes = 60;

            // prepare disk directory
            this._directoryOnDisk = PrepareTempDir();
        }

        public static Opml GetOpmlFeed(Uri uri)
        {
            return _theManager.GetFeed(uri);
        }

        internal Opml GetFeed(Uri uri)
        {
            Opml opmlFeed = null;

            lock (this._cache)
            {
                if (this._cache.TryGetValue(uri.AbsoluteUri, out opmlFeed))
                {
                    if (DateTime.UtcNow > opmlFeed.UtcExpiry)
                    {
                        this._cache.Remove(uri.AbsoluteUri);
                        opmlFeed = null;
                    }
                }
            }

            if (opmlFeed == null)
            {
                opmlFeed = this.DownloadOpmlFeed(uri);

                lock (this._cache)
                {
                    this._cache[uri.AbsoluteUri] = opmlFeed;
                }
            }

            return opmlFeed;
        }

        private static string PrepareTempDir()
        {
            string tempDir = null;

            try
            {
                string d = HttpContext.Current.Server.MapPath(Settings.CacheRoot + OPML_Dir);

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

        private static string GetTempFileNamePrefixFromUrl(Uri uri)
        {
            try
            {
                return string.Format("{0}_{1:x8}", uri.Host.Replace('.', '_'), uri.AbsolutePath.GetHashCode());
            }
            catch
            {
                return "opml";
            }
        }

        private Opml DownloadOpmlFeed(Uri uri)
        {
            // look for disk cache first
            Opml opmlFeed = this.TryLoadFromDisk(uri);

            if (opmlFeed != null)
            {
                return opmlFeed;
            }

            // May fail under partial trust
            try
            {
                byte[] feed = new WebClient().DownloadData(uri.AbsoluteUri);

                var opmlDoc = new XmlDocument { XmlResolver = null };
                opmlDoc.Load(new MemoryStream(feed));
                opmlFeed = Opml.LoadFromXml(opmlDoc);

                opmlFeed.UtcExpiry = DateTime.UtcNow.AddMinutes(this._defaultTtlMinutes);

                // save to disk
                this.TrySaveToDisk(opmlDoc, uri, opmlFeed.UtcExpiry);
            }
            catch
            {
                return new Opml();
            }

            return opmlFeed;
        }

        private Opml TryLoadFromDisk(Uri uri)
        {
            if (this._directoryOnDisk == null)
            {
                return null; // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(uri) + "_*.opml.resources";
            string[] files = Directory.GetFiles(this._directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

            foreach (string opmlFilename in files)
            {
                XmlDocument opmlDoc = null;
                bool isOpmlFileValid = false;
                DateTime utcExpiryFromOpmlFile = DateTime.MinValue;
                string urlFromOpmlFile = null;

                try
                {
                    opmlDoc = new XmlDocument { XmlResolver = null };
                    opmlDoc.Load(opmlFilename);

                    // look for special XML comment (before the root tag)'
                    // containing expiration and url
                    var comment = opmlDoc.DocumentElement.PreviousSibling as XmlComment;

                    if (comment != null)
                    {
                        string c = comment.Value;
                        int i = c.IndexOf('@');
                        long expiry;

                        if (long.TryParse(c.Substring(0, i), out expiry))
                        {
                            utcExpiryFromOpmlFile = DateTime.FromBinary(expiry);
                            urlFromOpmlFile = c.Substring(i + 1);
                            isOpmlFileValid = true;
                        }
                    }
                }
                catch
                {
                    // error processing one file shouldn't stop processing other files
                }

                // remove invalid or expired file
                if (!isOpmlFileValid || utcExpiryFromOpmlFile < DateTime.UtcNow)
                {
                    try
                    {
                        File.Delete(opmlFilename);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    // try next file
                    continue;
                }

                // match url
                if (urlFromOpmlFile.Equals(uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                {
                    // found a good one - create DOM and set expiry (as found on disk)
                    Opml opmlFeed = Opml.LoadFromXml(opmlDoc);
                    opmlFeed.UtcExpiry = utcExpiryFromOpmlFile;
                    return opmlFeed;
                }
            }

            // not found
            return null;
        }

        private void TrySaveToDisk(XmlDocument doc, Uri uri, DateTime utcExpiry)
        {
            if (this._directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment(string.Format("{0}@{1}", utcExpiry.ToBinary(), uri.AbsoluteUri)), doc.DocumentElement);

            string fileName = string.Format("{0}_{1:x8}.opml.resources", GetTempFileNamePrefixFromUrl(uri), Guid.NewGuid().ToString().GetHashCode());

            try
            {
                doc.Save(Path.Combine(this._directoryOnDisk, fileName));
            }
            catch
            {
                // can't save to disk - not a problem
            }
        }
    }
}
