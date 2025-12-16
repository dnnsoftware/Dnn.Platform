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

    /// <summary>Helper class that provides memory and disk caching of the downloaded feeds.</summary>
    internal sealed class OpmlDownloadManager
    {
        private const string OPMLDir = "/OPML/";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OpmlDownloadManager));
        private static readonly OpmlDownloadManager TheManager = new OpmlDownloadManager();

        private readonly Dictionary<string, Opml> cache;
        private readonly int defaultTtlMinutes;
        private readonly string directoryOnDisk;

        private OpmlDownloadManager()
        {
            // create in-memory cache
            this.cache = new Dictionary<string, Opml>();

            this.defaultTtlMinutes = 60;

            // prepare disk directory
            this.directoryOnDisk = PrepareTempDir();
        }

        /// <summary>Gets the OPML feed at a given <paramref name="uri"/>.</summary>
        /// <param name="uri">The feed URI.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        public static Opml GetOpmlFeed(Uri uri)
        {
            return TheManager.GetFeed(uri);
        }

        /// <summary>Gets the OPML feed at a given <paramref name="uri"/>.</summary>
        /// <param name="uri">The feed URI.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        internal Opml GetFeed(Uri uri)
        {
            Opml opmlFeed = null;

            lock (this.cache)
            {
                if (this.cache.TryGetValue(uri.AbsoluteUri, out opmlFeed))
                {
                    if (DateTime.UtcNow > opmlFeed.UtcExpiry)
                    {
                        this.cache.Remove(uri.AbsoluteUri);
                        opmlFeed = null;
                    }
                }
            }

            if (opmlFeed == null)
            {
                opmlFeed = this.DownloadOpmlFeed(uri);

                lock (this.cache)
                {
                    this.cache[uri.AbsoluteUri] = opmlFeed;
                }
            }

            return opmlFeed;
        }

        private static string PrepareTempDir()
        {
            string tempDir = null;

            try
            {
                string d = HttpContext.Current.Server.MapPath(Settings.CacheRoot + OPMLDir);

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
                return $"{uri.Host.Replace('.', '_')}_{uri.AbsolutePath.GetHashCode():x8}";
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

                opmlFeed.UtcExpiry = DateTime.UtcNow.AddMinutes(this.defaultTtlMinutes);

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
            if (this.directoryOnDisk == null)
            {
                return null; // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(uri) + "_*.opml.resources";
            string[] files = Directory.GetFiles(this.directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

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
            if (this.directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment($"{utcExpiry.ToBinary()}@{uri.AbsoluteUri}"), doc.DocumentElement);

            string fileName = $"{GetTempFileNamePrefixFromUrl(uri)}_{Guid.NewGuid().ToString().GetHashCode():x8}.opml.resources";

            try
            {
                doc.Save(Path.Combine(this.directoryOnDisk, fileName));
            }
            catch
            {
                // can't save to disk - not a problem
            }
        }
    }
}
