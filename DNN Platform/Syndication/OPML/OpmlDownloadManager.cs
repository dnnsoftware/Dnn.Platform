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
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Helper class that provides memory and disk caching of the downloaded feeds
    /// </summary>
    internal class OpmlDownloadManager
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (OpmlDownloadManager));
        private const string OPML_Dir = "/OPML/";
        private static readonly OpmlDownloadManager _theManager = new OpmlDownloadManager();

        private readonly Dictionary<string, Opml> _cache;
        private readonly int _defaultTtlMinutes;
        private readonly string _directoryOnDisk;

        private OpmlDownloadManager()
        {
            // create in-memory cache
            _cache = new Dictionary<string, Opml>();

            _defaultTtlMinutes = 60;

            // prepare disk directory
            _directoryOnDisk = PrepareTempDir();
        }

        public static Opml GetOpmlFeed(Uri uri)
        {
            return _theManager.GetFeed(uri);
        }

        internal Opml GetFeed(Uri uri)
        {
            Opml opmlFeed = null;

            lock (_cache)
            {
                if (_cache.TryGetValue(uri.AbsoluteUri, out opmlFeed))
                {
                    if (DateTime.UtcNow > opmlFeed.UtcExpiry)
                    {
                        _cache.Remove(uri.AbsoluteUri);
                        opmlFeed = null;
                    }
                }
            }

            if (opmlFeed == null)
            {
                opmlFeed = DownloadOpmlFeed(uri);

                lock (_cache)
                {
                    _cache[uri.AbsoluteUri] = opmlFeed;
                }
            }

            return opmlFeed;
        }

        private Opml DownloadOpmlFeed(Uri uri)
        {
            // look for disk cache first
            Opml opmlFeed = TryLoadFromDisk(uri);

            if (opmlFeed != null)
            {
                return (opmlFeed);
            }

            // May fail under partial trust
            try
            {
                byte[] feed = new WebClient().DownloadData(uri.AbsoluteUri);

                var opmlDoc = new XmlDocument();
                opmlDoc.Load(new MemoryStream(feed));
                opmlFeed = Opml.LoadFromXml(opmlDoc);

                opmlFeed.UtcExpiry = DateTime.UtcNow.AddMinutes(_defaultTtlMinutes);

                // save to disk
                TrySaveToDisk(opmlDoc, uri, opmlFeed.UtcExpiry);
            }
            catch
            {
                return (new Opml());
            }

            return (opmlFeed);
        }

        private Opml TryLoadFromDisk(Uri uri)
        {
            if (_directoryOnDisk == null)
            {
                return (null); // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(uri) + "_*.opml.resources";
            string[] files = Directory.GetFiles(_directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

            foreach (string opmlFilename in files)
            {
                XmlDocument opmlDoc = null;
                bool isOpmlFileValid = false;
                DateTime utcExpiryFromOpmlFile = DateTime.MinValue;
                string urlFromOpmlFile = null;

                try
                {
                    opmlDoc = new XmlDocument();
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
                if (urlFromOpmlFile.ToLower() == uri.AbsoluteUri.ToLower())
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
            if (_directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment(string.Format("{0}@{1}", utcExpiry.ToBinary(), uri.AbsoluteUri)), doc.DocumentElement);

            string fileName = string.Format("{0}_{1:x8}.opml.resources", GetTempFileNamePrefixFromUrl(uri), Guid.NewGuid().ToString().GetHashCode());

            try
            {
                doc.Save(Path.Combine(_directoryOnDisk, fileName));
            }
            catch
            {
                // can't save to disk - not a problem
            }
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
    }
}