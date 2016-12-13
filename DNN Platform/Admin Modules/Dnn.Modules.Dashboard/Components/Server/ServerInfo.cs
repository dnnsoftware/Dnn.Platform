#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common;

#endregion

namespace Dnn.Modules.Dashboard.Components.Server
{
    /// <summary>
    /// This class manages the Server Information for the site
    /// </summary>
    [XmlRoot("serverInfo")]
    public class ServerInfo : IXmlSerializable
    {
        public string Framework
        {
            get
            {
                return Environment.Version.ToString();
            }
        }

        public string HostName
        {
            get
            {
                return Dns.GetHostName();
            }
        }

        public string Identity
        {
            get
            {
                return WindowsIdentity.GetCurrent().Name;
            }
        }

        public string IISVersion
        {
            get
            {
                return HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"];
            }
        }

        public string OSVersion
        {
            get
            {
                return Environment.OSVersion.ToString();
            }
        }

        public string PhysicalPath
        {
            get
            {
                return Globals.ApplicationMapPath;
            }
        }

        public string Url
        {
            get
            {
                return Globals.GetDomainName(HttpContext.Current.Request);
            }
        }

        public string RelativePath
        {
            get
            {
                string path;
                if (string.IsNullOrEmpty(Globals.ApplicationPath))
                {
                    path = "/";
                }
                else
                {
                    path = Globals.ApplicationPath;
                }
                return path;
            }
        }

        public string ServerTime
        {
            get
            {
                return DateTime.Now.ToString();
            }
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("osVersion", OSVersion);
            writer.WriteElementString("iisVersion", IISVersion);
            writer.WriteElementString("framework", Framework);
            writer.WriteElementString("identity", Identity);
            writer.WriteElementString("hostName", HostName);
            writer.WriteElementString("physicalPath", PhysicalPath);
            writer.WriteElementString("url", Url);
            writer.WriteElementString("relativePath", RelativePath);
        }

        #endregion
    }
}
