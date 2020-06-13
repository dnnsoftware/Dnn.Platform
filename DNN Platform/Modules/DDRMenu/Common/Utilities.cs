// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Xsl;

    internal static class Utilities
    {
        private static readonly Dictionary<Type, XmlSerializer> serialisers = new Dictionary<Type, XmlSerializer>();

        internal static string CachedFileContent(string filename)
        {
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as string;
            if (result == null)
            {
                result = File.ReadAllText(filename);
                cache.Insert(filename, result, new CacheDependency(filename));
            }

            return result;
        }

        internal static XmlDocument CachedXml(string filename)
        {
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as XmlDocument;
            if (result == null)
            {
                result = new XmlDocument { XmlResolver = null };
                result.Load(filename);
                cache.Insert(filename, result, new CacheDependency(filename));
            }

            return result;
        }

        internal static XslCompiledTransform CachedXslt(string filename)
        {
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as XslCompiledTransform;
            if (result == null)
            {
                result = new XslCompiledTransform();
                using (var reader = XmlReader.Create(filename, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                {
                    result.Load(reader);
                }

                cache.Insert(filename, result, new CacheDependency(filename));
            }

            return result;
        }

        internal static XmlSerializer SerialiserFor(Type t)
        {
            XmlSerializer result;
            if (!serialisers.TryGetValue(t, out result))
            {
                lock (serialisers)
                {
                    result = (t.Name == "MenuXml")
                                ? new XmlSerializer(t, new XmlRootAttribute { ElementName = "Root" })
                                : new XmlSerializer(t);
                    if (!serialisers.ContainsKey(t))
                    {
                        serialisers.Add(t, result);
                    }
                }
            }

            return result;
        }

        internal static string ConvertToJs(object obj)
        {
            string result;

            if (obj == null)
            {
                return "null";
            }

            var objType = obj.GetType();
            if (objType == typeof(bool))
            {
                result = (bool)obj ? "true" : "false";
            }
            else if (objType == typeof(int) || objType == typeof(decimal) || objType == typeof(double))
            {
                result = obj.ToString();
            }
            else
            {
                result = string.Format("\"{0}\"", obj.ToString().Replace("\"", "\\\""));
            }

            return result;
        }
    }
}
