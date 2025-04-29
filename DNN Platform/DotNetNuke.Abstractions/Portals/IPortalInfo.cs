// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals;

using System;

/// <summary>The portal info.</summary>
public interface IPortalInfo
{
    /// <summary>Gets or sets the footer text as specified in the Portal settings.</summary>
    /// <value>Footer text of the portal.</value>
    /// <returns>Returns the the footer text of the portal.</returns>
    /// <remarks>
    /// <example>This show the usage of the <c>FooterText</c> property
    /// <code lang="vbnet">
    /// txtFooterText.Text = objPortal.FooterText
    /// </code>
    /// </example>
    /// </remarks>
    string FooterText { get; set; }

    /// <summary>Gets or sets home directory of the portal (logical path).</summary>
    /// <value>Portal home directory.</value>
    string HomeDirectory { get; set; }

    /// <summary>Gets home System (local) directory of the portal (logical path).</summary>
    /// <value>Portal home system directory.</value>
    string HomeSystemDirectory { get; }

    /// <summary>Gets fysical path on disk of the home directory of the portal.</summary>
    /// <value>Physical path on disk of the home directory of the portal.</value>
    string HomeDirectoryMapPath { get; }

    /// <summary>Gets fysical path on disk of the home directory of the portal.</summary>
    /// <value>Physical path on disk of the home directory of the portal.</value>
    string HomeSystemDirectoryMapPath { get; }

    /// <summary>Gets or sets userID of the user who is the admininistrator of the portal.</summary>
    /// <value>UserId of the user who is the portal admin.</value>
    int AdministratorId { get; set; }

    /// <summary>Gets or sets the RoleId of the Security Role of the Administrators group of the portal.</summary>
    /// <value>RoleId of de Administrators Security Role.</value>
    int AdministratorRoleId { get; set; }

    /// <summary>Gets or sets tabId at which admin tasks start.</summary>
    /// <value>TabID of admin tasks.</value>
    int AdminTabId { get; set; }

    /// <summary>Gets or sets current host version.</summary>
    string CrmVersion { get; set; }

    /// <summary>Gets or sets the Culture Code of the portal.</summary>
    string CultureCode { get; set; }

    /// <summary>Gets or sets default language for the portal.</summary>
    /// <value>Default language of the portal.</value>
    string DefaultLanguage { get; set; }

    /// <summary>Gets or sets description of the portal.</summary>
    /// <value>Description of the portal.</value>
    string Description { get; set; }

    /// <summary>Gets or sets the default e-mail to be used in the portal.</summary>
    /// <value>E-mail of the portal.</value>
    string Email { get; set; }

    /// <summary>Gets or sets date at which the portal expires.</summary>
    /// <value>Date of expiration of the portal.</value>
    DateTime ExpiryDate { get; set; }

    /// <summary>Gets or sets GUID of the portal info object.</summary>
    /// <value>Portal info Object GUID.</value>
    Guid GUID { get; set; }

    /// <summary>Gets or sets tabdId of the Home page.</summary>
    /// <value>TabId of the Home page.</value>
    int HomeTabId { get; set; }

    /// <summary>Gets or sets total disk space allowed for the portal (Mb). 0 means not limited.</summary>
    /// <value>Diskspace allowed for the portal.</value>
    int HostSpace { get; set; }

    /// <summary>Gets or sets keywords (separated by ,) for this portal.</summary>
    /// <value>Keywords seperated by .</value>
    string KeyWords { get; set; }

    /// <summary>Gets or sets tabId with the login control, page to login.</summary>
    /// <value>TabId of the Login page.</value>
    int LoginTabId { get; set; }

    /// <summary>Gets or sets the logo associated with the portal. The admin can set the logo in the portal settings.</summary>
    /// <value>URL of the logo.</value>
    string LogoFile { get; set; }

    /// <summary>Gets or sets number of portal pages allowed in the portal. 0 means not limited.</summary>
    /// <value>Number of portal pages allowed.</value>
    int PageQuota { get; set; }

    /// <summary>Gets or sets unique idenitifier of the Portal within the site.</summary>
    /// <value>Portal identifier.</value>
    int PortalId { get; set; }

    /// <summary>
    /// Gets or sets contains the id of the portal group that the portal belongs to
    /// Will be null or -1 (null.nullinteger) if the portal does not belong to a portal group.
    /// </summary>
    int PortalGroupId { get; set; }

    /// <summary>Gets or sets name of the portal. Can be set at creation time, Admin can change the name in the portal settings.</summary>
    /// <value>Name of the portal.</value>
    string PortalName { get; set; }

    /// <summary>Gets or sets the RoleId of the Registered users group of the portal.</summary>
    /// <value>RoleId of the Registered users. </value>
    int RegisteredRoleId { get; set; }

    /// <summary>Gets or sets tabid of the Registration page.</summary>
    /// <value>TabId of the Registration page.</value>
    int RegisterTabId { get; set; }

    /// <summary>Gets or sets tabid of the Search profile page.</summary>
    /// <value>TabdId of the Search Results page.</value>
    int SearchTabId { get; set; }

    /// <summary>Gets or sets tabid of the Custom 404 page.</summary>
    /// <value>Tabid of the Custom 404 page.</value>
    int Custom404TabId { get; set; }

    /// <summary>Gets or sets tabid of the Custom 500 error page.</summary>
    /// <value>Tabid of the Custom 500 error page.</value>
    int Custom500TabId { get; set; }

    /// <summary>Gets or sets tabid of the Terms of Use page.</summary>
    /// <value>Tabid of the Terms of Use page.</value>
    int TermsTabId { get; set; }

    /// <summary>Gets or sets tabid of the Privacy Statement page.</summary>
    /// <value>Tabid of the Privacy Statement page.</value>
    int PrivacyTabId { get; set; }

    /// <summary>Gets or sets tabdId of the splash page. If 0, there is no splash page.</summary>
    /// <value>TabdId of the Splash page.</value>
    int SplashTabId { get; set; }

    /// <summary>Gets or sets tabId at which Host tasks start.</summary>
    /// <value>TabId of Host tasks.</value>
    int SuperTabId { get; set; }

    /// <summary>Gets or sets number of registered users allowed in the portal. 0 means not limited.</summary>
    /// <value>Number of registered users allowed. </value>
    int UserQuota { get; set; }

    /// <summary>Gets or sets type of registration that the portal supports.</summary>
    /// <value>Type of registration.</value>
    int UserRegistration { get; set; }

    /// <summary>Gets or sets tabid of the User profile page.</summary>
    /// <value>TabdId of the User profile page.</value>
    int UserTabId { get; set; }

    /// <summary>
    /// Gets or sets the actual name of the Administrators group of the portal.
    /// This name is retrieved from the RoleController object.
    /// </summary>
    /// <value>The name of the Administrators group.</value>
    string AdministratorRoleName { get; set; }

    /// <summary>
    /// Gets or sets the actual name of the Registerd Users group of the portal.
    /// This name is retrieved from the RoleController object.
    /// </summary>
    /// <value>The name of the Registerd Users group.</value>
    string RegisteredRoleName { get; set; }
}
