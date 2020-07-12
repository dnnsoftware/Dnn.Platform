// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;

    public interface IPortalAliasInfo
    {
        //BrowserTypes BrowserType { get; set; }
        string CultureCode { get; set; }
        string HTTPAlias { get; set; }
        bool IsPrimary { get; set; }
        int KeyID { get; set; }
        int PortalAliasID { get; set; }
        int PortalID { get; set; }
        bool Redirect { get; set; }
        string Skin { get; set; }

        void Fill(IDataReader dr);
        XmlSchema GetSchema();
        void ReadXml(XmlReader reader);
        void WriteXml(XmlWriter writer);
    }
}
