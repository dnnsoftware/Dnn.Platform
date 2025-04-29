// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using DotNetNuke.Abstractions.Portals.Templates;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

public class PortalTemplateInfo : IPortalTemplateInfo
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
    private string resourceFilePath;

    /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
    /// <param name="templateFilePath"></param>
    /// <param name="cultureCode"></param>
    public PortalTemplateInfo(string templateFilePath, string cultureCode)
    {
        this.TemplateFilePath = templateFilePath;

        this.InitLocalizationFields(cultureCode);
        this.InitNameAndDescription();
    }

    public string ResourceFilePath
    {
        get
        {
            if (this.resourceFilePath == null)
            {
                this.resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(this.TemplateFilePath);
            }

            return this.resourceFilePath;
        }
    }

    public string Name { get; private set; }

    public string CultureCode { get; private set; }

    public string TemplateFilePath { get; private set; }

    public string LanguageFilePath { get; private set; }

    public string Description { get; private set; }

    private static string ReadLanguageFileValue(XDocument xmlDoc, string name)
    {
        return (from f in xmlDoc.Descendants("data")
            where (string)f.Attribute("name") == name
            select (string)f.Element("value")).SingleOrDefault();
    }

    private void InitNameAndDescription()
    {
        if (!string.IsNullOrEmpty(this.LanguageFilePath))
        {
            this.LoadNameAndDescriptionFromLanguageFile();
        }

        if (string.IsNullOrEmpty(this.Name))
        {
            this.Name = Path.GetFileNameWithoutExtension(this.TemplateFilePath);
        }

        if (string.IsNullOrEmpty(this.Description))
        {
            this.LoadDescriptionFromTemplateFile();
        }
    }

    private void LoadDescriptionFromTemplateFile()
    {
        try
        {
            XDocument xmlDoc;
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.TemplateFilePath))
            {
                xmlDoc = XDocument.Load(reader);
            }

            this.Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
        }
        catch (Exception e)
        {
            Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
        }
    }

    private void LoadNameAndDescriptionFromLanguageFile()
    {
        try
        {
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.LanguageFilePath))
            {
                var xmlDoc = XDocument.Load(reader);

                this.Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                this.Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
            }
        }
        catch (Exception e)
        {
            Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
        }
    }

    private void InitLocalizationFields(string cultureCode)
    {
        this.LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(this.TemplateFilePath, cultureCode);
        if (string.IsNullOrEmpty(this.LanguageFilePath))
        {
            var locales = new List<string>();
            (cultureCode, locales) = PortalTemplateIO.Instance.GetTemplateLanguages(this.TemplateFilePath);
            if (string.IsNullOrEmpty(cultureCode))
            {
                var portalSettings = PortalSettings.Current;
                cultureCode = portalSettings != null ? PortalController.GetPortalDefaultLanguage(portalSettings.PortalId) : Localization.SystemLocale;
            }
        }

        this.CultureCode = cultureCode;
    }
}
