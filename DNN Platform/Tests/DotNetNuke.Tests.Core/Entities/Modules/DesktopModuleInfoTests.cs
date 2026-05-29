// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

using NUnit.Framework;

[TestFixture]
public class DesktopModuleInfoTests
{
    [Test]
    public void ReadXml_WithUpgradeable_ReadsOneSupportedFeature()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.True);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.False);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
    }

    [Test]
    public void ReadXml_WithPortable_ReadsOneSupportedFeature()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.True);
            Assert.That(roleGroup.IsSearchable, Is.False);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable));
        }
    }

    [Test]
    public void ReadXml_WithSearchable_ReadsOneSupportedFeature()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.True);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable));
        }
    }

    [Test]
    public void ReadXml_WithSearchableAndPortable_ReadsTwoSupportedFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                    <supportedFeature type="Portable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.True);
            Assert.That(roleGroup.IsSearchable, Is.True);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable + (int)DesktopModuleSupportedFeature.IsPortable));
        }
    }

    [Test]
    public void ReadXml_WithSearchableAndUpgradeable_ReadsTwoSupportedFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.True);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.True);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable + (int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
    }

    [Test] public void ReadXml_WithPortableAndUpgradeable_ReadsTwoSupportedFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.True);
            Assert.That(roleGroup.IsPortable, Is.True);
            Assert.That(roleGroup.IsSearchable, Is.False);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable + (int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
    }

    [Test] public void ReadXml_WithAllThreeFeatures_ReadsThreeSupportedFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                    <supportedFeature type="Upgradeable" />
                    <supportedFeature type="Searchable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.IsUpgradeable, Is.True);
            Assert.That(roleGroup.IsPortable, Is.True);
            Assert.That(roleGroup.IsSearchable, Is.True);
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable + (int)DesktopModuleSupportedFeature.IsUpgradeable + (int)DesktopModuleSupportedFeature.IsSearchable));
        }
    }

    [Test]
    public void ReadXml_WithEmptySupportedFeatures_DoesNotInitializeFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures></supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.False);
        }
    }

    [Test]
    public void ReadXml_WithEmptySupportedFeaturesIncludingWhiteSpace_DoesNotInitializeFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.False);
        }
    }

    [Test]
    public void ReadXml_WithNoSupportedFeatures_DoesNotInitializeFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.False);
        }
    }

    [Test]
    public void ReadXml_WithSelfClosingSupportedFeatures_DoesNotInitializeFeatures()
    {
        var roleGroup = ReadXml(
            """
            <desktopModule>
                <supportedFeatures />
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(roleGroup.IsUpgradeable, Is.False);
            Assert.That(roleGroup.IsPortable, Is.False);
            Assert.That(roleGroup.IsSearchable, Is.False);
        }
    }

    private static DesktopModuleInfo ReadXml([StringSyntax(StringSyntaxAttribute.Xml)] string xml)
    {
        var desktopModule = new DesktopModuleInfo();

        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader);
        xmlReader.Read();
        desktopModule.ReadXml(xmlReader);

        return desktopModule;
    }
}
