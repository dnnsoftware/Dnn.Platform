//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
namespace DotNetNuke.Tests.UI.WatiN.Utilities;

using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Web.Configuration;

using DotNetNuke.Services.Installer;

public static class WebConfigManager
{
    public static string GetWebPath()
    {
        var webPath = Directory.GetCurrentDirectory().Replace("\\Tests\\Fixtures", "\\Website");

        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultPhysicalAppPath"]))
        {
            webPath = ConfigurationManager.AppSettings["DefaultPhysicalAppPath"];
        }

        return webPath;
    }

    /// <summary>Updates the web.config so that the site will run in full trust.</summary>
    public static void UpdateConfigForFullTrust()
    {
        var physicalPath = GetWebPath();
        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Full";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
    }

    /// <summary>Updates the web.config so that the site will run in medium trust.</summary>
    public static void UpdateConfigForMediumTrust()
    {
        var physicalPath = GetWebPath();
        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Medium";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
    }

    /// <summary>Updates the web.config file to drop emails to a local folder.</summary>
    /// <param name="mailDropPath">The path to the mailDrop.xml file that contains the xml for the mail dump.</param>
    /// <param name="emailPath">The path that emails will be sent to.</param>
    public static void UpdateConfigForMailDrop(string mailDropPath, string emailPath)
    {
        var physicalPath = GetWebPath();
        var mailDropFragment = XDocument.Load(Path.Combine(mailDropPath, "mailDrop.xml"));
        var specifiedPickupDirectory = mailDropFragment.XPathSelectElement("configuration/nodes/node/system.net/mailSettings/smtp/specifiedPickupDirectory");
        specifiedPickupDirectory.Attribute("pickupDirectoryLocation").Value = emailPath;
        mailDropFragment.Save(Path.Combine(physicalPath, "UpdatedMailDrop.xml"));
        var mailDrop = new FileStream(Path.Combine(physicalPath, "UpdatedMailDrop.xml"), FileMode.Open, FileAccess.Read);
        try
        {
            var fileName = $"{physicalPath}\\web.config";
            var targetDocument = new XmlDocument();
            targetDocument.Load(fileName);
            var mailNodes = (from mail in targetDocument.DocumentElement.ChildNodes.Cast<XmlNode>()
                where mail.Name == "system.net"
                select mail).ToList();
            if (mailNodes.Count == 0)
            {
                var merge = new XmlMerge(mailDrop, string.Empty, string.Empty);
                merge.UpdateConfig(targetDocument);
                targetDocument.Save(fileName);
            }
        }
        finally
        {
            mailDrop.Close();
        }
    }

    /// <summary>Updates the web.config so that the site will run in full trust.</summary>
    /// <param name="physicalPath">The path for the folder containing the web.config.</param>
    public static void UpdateConfigForFullTrust(string physicalPath)
    {

        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Full";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
    }

    public static void SyncConfig(string sitePath)
    {
        var configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        var webConfigPath = Path.Combine(sitePath, "web.config");

        var dllConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var webConfig = XDocument.Load(webConfigPath);

        var keySection = dllConfig.GetSection("system.web/machineKey") as MachineKeySection;
        var webNode = webConfig.XPathSelectElement("configuration/system.web/machineKey");

        if (keySection.ValidationKey != webNode.Attribute("validationKey").Value)
        {
            keySection.ValidationKey = webNode.Attribute("validationKey").Value;
            keySection.DecryptionKey = webNode.Attribute("decryptionKey").Value;

            dllConfig.Save();

            Type type = Assembly.GetAssembly(typeof(System.Web.TraceContext)).GetType("System.Web.Configuration.RuntimeConfig");
            var fieldInfo = type.GetField("s_clientRuntimeConfig", BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo.SetValue(null, null);
            ConfigurationManager.RefreshSection("system.web/machineKey");
        }
    }

    public static void TouchConfig(string sitePath)
    {
        var webConfigPath = Path.Combine(sitePath, "web.config");
        File.SetLastWriteTime(webConfigPath, DateTime.Now);
    }
}
