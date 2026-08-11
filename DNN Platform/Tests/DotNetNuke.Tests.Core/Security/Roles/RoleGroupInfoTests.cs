// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Roles;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

using DotNetNuke.Security.Roles;

using NUnit.Framework;

[TestFixture]
public class RoleGroupInfoTests
{
    [Test]
    public void ReadXml_WithRealisticXml_ReadsRoles()
    {
        var roleGroup = ReadXml(
            """
            <rolegroups>
              <rolegroup>
                <rolegroupname>GlobalRoles</rolegroupname>
                <description>A dummy role group that represents the Global roles</description>
                <roles>
                  <role>
                    <rolename>Administrators</rolename>
                    <description>Administrators of this Website</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>adminrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Registered Users</rolename>
                    <description>Registered Users</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>true</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>registeredrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Subscribers</rolename>
                    <description>A public role for site subscriptions</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>true</ispublic>
                    <autoassignment>true</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>subscriberrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Translator (en-US)</rolename>
                    <description>A role for English (United States) translators</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>false</issystemrole>
                    <roletype>none</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Unverified Users</rolename>
                    <description>Unverified Users</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>unverifiedrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                </roles>
              </rolegroup>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("GlobalRoles"));
            Assert.That(roleGroup.Description, Is.EqualTo("A dummy role group that represents the Global roles"));
            Assert.That(roleGroup.Roles, Has.Count.EqualTo(5));
        }
    }

    [Test]
    public void ReadXml_WithEmptyRoles_ReadsZeroRoles()
    {
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles></roles>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
    }

    [Test]
    public void ReadXml_WithEmptyRolesWithWhiteSpace_ReadsZeroRoles()
    {
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles>
                </roles>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
    }

    [Test]
    public void ReadXml_WithSelfClosingRoles_ReadsZeroRoles()
    {
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles />
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
    }

    [Test]
    public void ReadXml_WithNoRoles_ReadsZeroRoles()
    {
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
    }

    private static RoleGroupInfo ReadXml([StringSyntax(StringSyntaxAttribute.Xml)] string xml)
    {
        var roleGroup = new RoleGroupInfo();

        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader);
        xmlReader.Read();
        roleGroup.ReadXml(xmlReader);

        return roleGroup;
    }
}
