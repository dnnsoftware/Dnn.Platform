// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using Newtonsoft.Json;

    /// <summary>
    /// PortalInfo provides a base class for Portal information
    /// This class inherits from the <c>BaseEntityInfo</c> and is <c>Hydratable</c>.
    /// </summary>
    /// <remarks><seealso cref="IHydratable"/>
    /// <example>This example shows how the <c>PortalInfo</c> class is used to get physical file names
    ///  <code lang="vbnet">
    /// Public ReadOnly Property PhysicalPath() As String
    ///        Get
    ///            Dim _PhysicalPath As String
    ///            Dim PortalSettings As PortalSettings = Nothing
    ///            If Not HttpContext.Current Is Nothing Then
    ///                PortalSettings = PortalController.Instance.GetCurrentPortalSettings()
    ///            End If
    ///            If PortalId = Null.NullInteger Then
    ///                _PhysicalPath = DotNetNuke.Common.Globals.HostMapPath + RelativePath
    ///            Else
    ///                If PortalSettings Is Nothing OrElse PortalSettings.PortalId &lt;&gt; PortalId Then
    ///                    ' Get the PortalInfo  based on the Portalid
    ///                    Dim objPortals As New PortalController()
    ///                    Dim objPortal As PortalInfo = objPortals.GetPortal(PortalId)
    ///                    _PhysicalPath = objPortal.HomeDirectoryMapPath + RelativePath
    ///                Else
    ///                    _PhysicalPath = PortalSettings.HomeDirectoryMapPath + RelativePath
    ///                End If
    ///            End If
    ///            Return _PhysicalPath.Replace("/", "\")
    ///        End Get
    /// End Property
    /// </code>
    /// </example>
    /// </remarks>
    [XmlRoot("settings", IsNullable = false)]
    [Serializable]
    public class PortalInfo : BaseEntityInfo, IHydratable, IPortalInfo
    {
        private string administratorRoleName;
        private int pages = Null.NullInteger;
        private string registeredRoleName;
        private PortalPermissionCollection permissions;

        private int users;

        /// <summary>Initializes a new instance of the <see cref="PortalInfo"/> class.</summary>
        /// <remarks>
        /// <example>This example illustrates the creation of a new <c>PortalInfo</c> object
        /// <code lang="vbnet">
        /// For Each portal As PortalInfo In New PortalController().GetPortals
        ///     Dim portalID As Integer = portal.PortalID
        ///     ...
        /// Next
        /// </code>
        /// </example>
        /// </remarks>
        public PortalInfo()
        {
            this.Users = Null.NullInteger;
        }

        /// <inheritdoc />
        [XmlElement("footertext")]
        public string FooterText { get; set; }

        /// <inheritdoc />
        [XmlElement("homedirectory")]
        public string HomeDirectory { get; set; }

        /// <inheritdoc />
        [XmlElement("homesystemdirectory")]
        public string HomeSystemDirectory => $"{this.HomeDirectory}-System";

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        public string HomeDirectoryMapPath => $@"{Globals.ApplicationMapPath}\{this.HomeDirectory.Replace("/", @"\")}\";

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        public string HomeSystemDirectoryMapPath => $@"{Globals.ApplicationMapPath}\{this.HomeSystemDirectory.Replace("/", @"\")}\";

        /// <inheritdoc />
        [XmlElement("administratorid")]
        public int AdministratorId { get; set; }

        /// <inheritdoc />
        [XmlElement("administratorroleid")]
        public int AdministratorRoleId { get; set; }

        /// <inheritdoc />
        [XmlElement("admintabid")]
        public int AdminTabId { get; set; }

        /// <summary>Gets or sets image (bitmap) file that is used as background for the portal.</summary>
        /// <value>Name of the file that is used as background.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("backgroundfile")]
        public string BackgroundFile { get; set; }

        /// <inheritdoc />
        [XmlElement("crmversion")]
        public string CrmVersion { get; set; }

        /// <summary>Gets or sets setting for the type of banner advertising in the portal.</summary>
        /// <value>Type of banner advertising.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("banneradvertising")]
        public int BannerAdvertising { get; set; }

        /// <inheritdoc />
        [XmlElement("cultureCode")]
        public string CultureCode { get; set; }

        /// <summary>Gets or sets currency format that is used in the portal.</summary>
        /// <value>Currency of the portal.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("currency")]
        public string Currency { get; set; }

        /// <inheritdoc />
        [XmlElement("defaultlanguage")]
        public string DefaultLanguage { get; set; }

        /// <inheritdoc />
        [XmlElement("description")]
        public string Description { get; set; }

        /// <inheritdoc />
        [XmlElement("email")]
        public string Email { get; set; }

        /// <inheritdoc />
        [XmlElement("expirydate")]
        public DateTime ExpiryDate { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
        public Guid GUID { get; set; }

        /// <inheritdoc />
        [XmlElement("hometabid")]
        public int HomeTabId { get; set; }

        /// <summary>Gets or sets amount of currency that is used as a hosting fee of the portal.</summary>
        /// <value>Currency amount hosting fee.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("hostfee")]
        public float HostFee { get; set; }

        /// <inheritdoc />
        [XmlElement("hostspace")]
        public int HostSpace { get; set; }

        /// <inheritdoc />
        [XmlElement("keywords")]
        public string KeyWords { get; set; }

        /// <inheritdoc />
        [XmlElement("logintabid")]
        public int LoginTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("logofile")]
        public string LogoFile { get; set; }

        /// <inheritdoc />
        [XmlElement("pagequota")]
        public int PageQuota { get; set; }

        /// <summary>Gets or sets name of the Payment processor that is used for portal payments, e.g. PayPal.</summary>
        /// <value>Name of the portal payment processor.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("paymentprocessor")]
        public string PaymentProcessor { get; set; }

        /// <inheritdoc />
        [XmlElement("portalid")]
        int IPortalInfo.PortalId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalInfo.PortalId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalID
        {
            get => this.ThisAsInterface.PortalId;
            set => this.ThisAsInterface.PortalId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <summary>Gets the permissions collection for the portal.</summary>
        [XmlArray("portalpermissions")]
        [XmlArrayItem("permission")]
        public PortalPermissionCollection PortalPermissions
        {
            get
            {
                return this.permissions ?? (this.permissions = new PortalPermissionCollection(PortalPermissionController.GetPortalPermissions(this.ThisAsInterface.PortalId)));
            }
        }

        /// <inheritdoc />
        int IPortalInfo.PortalGroupId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalInfo.PortalGroupId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalGroupID
        {
            get => this.ThisAsInterface.PortalGroupId;
            set => this.ThisAsInterface.PortalGroupId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        [XmlElement("portalname")]
        public string PortalName { get; set; }

        /// <summary>Gets or sets password to use in the payment processor.</summary>
        /// <value>Payment Processor password.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("processorpassword")]
        public string ProcessorPassword { get; set; }

        /// <summary>Gets or sets payment Processor userId.</summary>
        /// <value>
        /// Payment Processor userId.
        /// </value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("processoruserid")]
        public string ProcessorUserId { get; set; }

        /// <inheritdoc />
        [XmlElement("registeredroleid")]
        public int RegisteredRoleId { get; set; }

        /// <inheritdoc />
        [XmlElement("registertabid")]
        public int RegisterTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("searchtabid")]
        public int SearchTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("custom404tabid")]
        public int Custom404TabId { get; set; }

        /// <inheritdoc />
        [XmlElement("custom500tabid")]
        public int Custom500TabId { get; set; }

        /// <inheritdoc />
        [XmlElement("termstabid")]
        public int TermsTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("privacytabid")]
        public int PrivacyTabId { get; set; }

        [XmlElement("siteloghistory")]
        [Obsolete("Deprecated in DotNetNuke 8.0.0. No replacement. Scheduled removal in v11.0.0.")]
        public int SiteLogHistory { get; set; }

        /// <inheritdoc />
        [XmlElement("splashtabid")]
        public int SplashTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("supertabid")]
        public int SuperTabId { get; set; }

        /// <inheritdoc />
        [XmlElement("userquota")]
        public int UserQuota { get; set; }

        /// <inheritdoc />
        [XmlElement("userregistration")]
        public int UserRegistration { get; set; }

        /// <inheritdoc />
        [XmlElement("usertabid")]
        public int UserTabId { get; set; }

        /// <summary>Gets or sets actual number of actual users for this portal.</summary>
        /// <value>Number of users for the portal.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("users")]
        public int Users
        {
            get
            {
                if (this.users < 0)
                {
                    this.users = UserController.GetUserCountByPortal(this.ThisAsInterface.PortalId);
                }

                return this.users;
            }

            set
            {
                this.users = value;
            }
        }

        /// <summary>Gets or sets dNN Version # of the portal installation.</summary>
        /// <value>Version # of the portal installation.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("version")]
        public string Version { get; set; }

        /// <inheritdoc />
        [XmlElement("administratorrolename")]
        public string AdministratorRoleName
        {
            get
            {
                if (this.administratorRoleName == Null.NullString && this.AdministratorRoleId > Null.NullInteger)
                {
                    // Get Role Name
                    RoleInfo adminRole = RoleController.Instance.GetRole(this.ThisAsInterface.PortalId, r => r.RoleID == this.AdministratorRoleId);
                    if (adminRole != null)
                    {
                        this.administratorRoleName = adminRole.RoleName;
                    }
                }

                return this.administratorRoleName;
            }

            set
            {
                this.administratorRoleName = value;
            }
        }

        /// <summary>Gets or sets actual number of pages of the portal.</summary>
        /// <value>Number of pages of the portal.</value>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. No replacement. Scheduled removal in v11.0.0.")]
        [XmlElement("pages")]
        public int Pages
        {
            get
            {
                if (this.pages < 0)
                {
                    this.pages = TabController.Instance.GetUserTabsByPortal(this.ThisAsInterface.PortalId).Count;
                }

                return this.pages;
            }

            set
            {
                this.pages = value;
            }
        }

        /// <inheritdoc />
        [XmlElement("registeredrolename")]
        public string RegisteredRoleName
        {
            get
            {
                if (this.registeredRoleName == Null.NullString && this.RegisteredRoleId > Null.NullInteger)
                {
                    // Get Role Name
                    RoleInfo regUsersRole = RoleController.Instance.GetRole(this.ThisAsInterface.PortalId, r => r.RoleID == this.RegisteredRoleId);
                    if (regUsersRole != null)
                    {
                        this.registeredRoleName = regUsersRole.RoleName;
                    }
                }

                return this.registeredRoleName;
            }

            set
            {
                this.registeredRoleName = value;
            }
        }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.ThisAsInterface.PortalId;
            }

            set
            {
                this.ThisAsInterface.PortalId = value;
            }
        }

        private IPortalInfo ThisAsInterface => this;

        /// <summary>Fills a PortalInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// <remarks>Standard IHydratable.Fill implementation.
        /// <seealso cref="KeyID"></seealso></remarks>
        public void Fill(IDataReader dr)
        {
            this.ThisAsInterface.PortalId = Null.SetNullInteger(dr["PortalID"]);

            try
            {
                this.ThisAsInterface.PortalGroupId = Null.SetNullInteger(dr["PortalGroupID"]);
            }
            catch (IndexOutOfRangeException)
            {
                if (Globals.Status == Globals.UpgradeStatus.None)
                {
                    // this should not happen outside of an upgrade situation
                    throw;
                }

                // else swallow the error
            }

            this.PortalName = Null.SetNullString(dr["PortalName"]);
            this.LogoFile = Null.SetNullString(dr["LogoFile"]);
            this.FooterText = Null.SetNullString(dr["FooterText"]);
            this.ExpiryDate = Null.SetNullDateTime(dr["ExpiryDate"]);
            this.UserRegistration = Null.SetNullInteger(dr["UserRegistration"]);
            this.BannerAdvertising = Null.SetNullInteger(dr["BannerAdvertising"]);
            this.AdministratorId = Null.SetNullInteger(dr["AdministratorID"]);
            this.Email = Null.SetNullString(dr["Email"]);
            this.Currency = Null.SetNullString(dr["Currency"]);
            this.HostFee = Null.SetNullInteger(dr["HostFee"]);
            this.HostSpace = Null.SetNullInteger(dr["HostSpace"]);
            this.PageQuota = Null.SetNullInteger(dr["PageQuota"]);
            this.UserQuota = Null.SetNullInteger(dr["UserQuota"]);
            this.AdministratorRoleId = Null.SetNullInteger(dr["AdministratorRoleID"]);
            this.RegisteredRoleId = Null.SetNullInteger(dr["RegisteredRoleID"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.KeyWords = Null.SetNullString(dr["KeyWords"]);
            this.BackgroundFile = Null.SetNullString(dr["BackGroundFile"]);
            this.GUID = new Guid(Null.SetNullString(dr["GUID"]));
            this.PaymentProcessor = Null.SetNullString(dr["PaymentProcessor"]);
            this.ProcessorUserId = Null.SetNullString(dr["ProcessorUserId"]);
            var p = Null.SetNullString(dr["ProcessorPassword"]);
            try
            {
                this.ProcessorPassword = string.IsNullOrEmpty(p)
                    ? p
                    : Security.FIPSCompliant.DecryptAES(p, Config.GetDecryptionkey(), Host.Host.GUID);
            }
            catch (Exception ex) when (ex is FormatException || ex is CryptographicException)
            {
                // for backward compatibility
                this.ProcessorPassword = p;
            }

            this.SplashTabId = Null.SetNullInteger(dr["SplashTabID"]);
            this.HomeTabId = Null.SetNullInteger(dr["HomeTabID"]);
            this.LoginTabId = Null.SetNullInteger(dr["LoginTabID"]);
            this.RegisterTabId = Null.SetNullInteger(dr["RegisterTabID"]);
            this.UserTabId = Null.SetNullInteger(dr["UserTabID"]);
            this.SearchTabId = Null.SetNullInteger(dr["SearchTabID"]);

            this.Custom404TabId = this.Custom500TabId = Null.NullInteger;
            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'Custom404TabId'").Length > 0)
                {
                    this.Custom404TabId = Null.SetNullInteger(dr["Custom404TabId"]);
                }

                if (schema.Select("ColumnName = 'Custom500TabId'").Length > 0)
                {
                    this.Custom500TabId = Null.SetNullInteger(dr["Custom500TabId"]);
                }
            }

            this.TermsTabId = Null.SetNullInteger(dr["TermsTabId"]);
            this.PrivacyTabId = Null.SetNullInteger(dr["PrivacyTabId"]);

            this.DefaultLanguage = Null.SetNullString(dr["DefaultLanguage"]);
            this.AdminTabId = Null.SetNullInteger(dr["AdminTabID"]);
            this.HomeDirectory = Null.SetNullString(dr["HomeDirectory"]);
            this.SuperTabId = Null.SetNullInteger(dr["SuperTabId"]);
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);

            this.FillInternal(dr);
            this.AdministratorRoleName = Null.NullString;
            this.RegisteredRoleName = Null.NullString;

            this.Users = Null.NullInteger;
            this.Pages = Null.NullInteger;
        }
    }
}
