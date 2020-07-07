// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Data;
    using System.Security.Cryptography;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    /// <summary>
    /// PortalInfo provides a base class for Portal information
    /// This class inherites from the <c>BaseEntityInfo</c> and is <c>Hydratable</c>.
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
    public class PortalInfo : BaseEntityInfo, IHydratable
    {
        private string _administratorRoleName;
        private int _pages = Null.NullInteger;
        private string _registeredRoleName;

        private int _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalInfo"/> class.
        /// Create new Portalinfo instance.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the footer text as specified in the Portal settings.
        /// </summary>
        /// <value>Footer text of the portal.</value>
        /// <returns>Returns the the footer text of the portal.</returns>
        /// <remarks>
        /// <example>This show the usage of the <c>FooterText</c> property
        /// <code lang="vbnet">
        /// txtFooterText.Text = objPortal.FooterText
        /// </code>
        /// </example>
        /// </remarks>
        [XmlElement("footertext")]
        public string FooterText { get; set; }

        /// <summary>
        /// Gets or sets home directory of the portal (logical path).
        /// </summary>
        /// <value>Portal home directory.</value>
        /// <returns>Portal home directory.</returns>
        /// <remarks><seealso cref="HomeDirectoryMapPath"></seealso></remarks>
        [XmlElement("homedirectory")]
        public string HomeDirectory { get; set; }

        /// <summary>
        /// Gets home System (local) directory of the portal (logical path).
        /// </summary>
        /// <value>Portal home system directory.</value>
        /// <returns>Portal home system directory in local filesystem.</returns>
        /// <remarks><seealso cref="HomeSystemDirectoryMapPath"></seealso></remarks>
        [XmlElement("homesystemdirectory")]
        public string HomeSystemDirectory
        {
            get { return string.Format("{0}-System", this.HomeDirectory); }
        }

        /// <summary>
        /// Gets fysical path on disk of the home directory of the portal.
        /// </summary>
        /// <value>
        /// <placeholder>Fysical path on disk of the home directory of the portal</placeholder>
        /// </value>
        /// <returns>Fully qualified path of the home directory.</returns>
        /// <remarks><seealso cref="HomeDirectory"></seealso></remarks>
        [XmlIgnore]
        public string HomeDirectoryMapPath
        {
            get
            {
                return string.Format("{0}\\{1}\\", Globals.ApplicationMapPath, this.HomeDirectory.Replace("/", "\\"));
            }
        }

        /// <summary>
        /// Gets fysical path on disk of the home directory of the portal.
        /// </summary>
        /// <value>
        /// <placeholder>Fysical path on disk of the home directory of the portal</placeholder>
        /// </value>
        /// <returns>Fully qualified path of the home system (local) directory.</returns>
        /// <remarks><seealso cref="HomeDirectory"></seealso></remarks>
        [XmlIgnore]
        public string HomeSystemDirectoryMapPath
        {
            get
            {
                return string.Format("{0}\\{1}\\", Globals.ApplicationMapPath, this.HomeSystemDirectory.Replace("/", "\\"));
            }
        }

        /// <summary>
        /// Gets or sets userID of the user who is the admininistrator of the portal.
        /// </summary>
        /// <value>UserId of the user who is the portal admin.</value>
        /// <returns>UserId of the user who is the portal admin.</returns>
        /// <remarks><example>This show the usage of the <c>AdministratorId</c>
        /// <code lang="vbnet">
        /// Dim Arr As ArrayList = objRoleController.GetUserRolesByRoleName(intPortalId, objPortal.AdministratorRoleName)
        /// Dim i As Integer
        ///       For i = 0 To Arr.Count - 1
        ///             Dim objUser As UserRoleInfo = CType(Arr(i), UserRoleInfo)
        ///             cboAdministratorId.Items.Add(New ListItem(objUser.FullName, objUser.UserID.ToString))
        ///      Next
        ///      If Not cboAdministratorId.Items.FindByValue(objPortal.AdministratorId.ToString) Is Nothing Then
        ///          cboAdministratorId.Items.FindByValue(objPortal.AdministratorId.ToString).Selected = True
        ///      End If
        /// </code></example></remarks>
        [XmlElement("administratorid")]
        public int AdministratorId { get; set; }

        /// <summary>
        /// Gets or sets the RoleId of the Security Role of the Administrators group of the portal.
        /// </summary>
        /// <value>RoleId of de Administrators Security Role.</value>
        /// <returns>RoleId of de Administrators Security Role.</returns>
        /// <remarks><example>This shows the usage of the AdministratoprRoleId
        /// <code lang="vbnet">
        /// Dim objPortal As PortalInfo = New PortalController().GetPortal(PortalID)
        ///     If RoleID = objPortal.AdministratorRoleId Then
        ///         _RoleType = Roles.RoleType.Administrator
        ///     ElseIf RoleID = objPortal.RegisteredRoleId Then
        ///         _RoleType = Roles.RoleType.RegisteredUser
        ///     ElseIf RoleName = "Subscribers" Then
        ///         _RoleType = Roles.RoleType.Subscriber
        ///     End If
        /// </code>
        /// </example>
        /// </remarks>
        [XmlElement("administratorroleid")]
        public int AdministratorRoleId { get; set; }

        /// <summary>
        /// Gets or sets tabId at which admin tasks start.
        /// </summary>
        /// <value>TabID of admin tasks.</value>
        /// <returns>TabID of admin tasks.</returns>
        /// <remarks></remarks>
        [XmlElement("admintabid")]
        public int AdminTabId { get; set; }

        /// <summary>
        /// Gets or sets image (bitmap) file that is used as background for the portal.
        /// </summary>
        /// <value>Name of the file that is used as background.</value>
        /// <returns>Name of the file that is used as background.</returns>
        /// <remarks></remarks>
        [XmlElement("backgroundfile")]
        public string BackgroundFile { get; set; }

        /// <summary>
        /// Gets or sets current host version.
        /// </summary>
        [XmlElement("crmversion")]
        public string CrmVersion { get; set; }

        /// <summary>
        /// Gets or sets setting for the type of banner advertising in the portal.
        /// </summary>
        /// <value>Type of banner advertising.</value>
        /// <returns>Type of banner advertising.</returns>
        /// <remarks><example>This show the usage of BannerAdvertising setting
        /// <code lang="vbnet">
        /// optBanners.SelectedIndex = objPortal.BannerAdvertising
        /// </code></example></remarks>
        [XmlElement("banneradvertising")]
        public int BannerAdvertising { get; set; }

        [XmlElement("cultureCode")]
        public string CultureCode { get; set; }

        /// <summary>
        /// Gets or sets curreny format that is used in the portal.
        /// </summary>
        /// <value>Currency of the portal.</value>
        /// <returns>Currency of the portal.</returns>
        /// <remarks><example>This exampels show the usage of the Currentcy property
        /// <code lang="vbnet">
        /// cboCurrency.DataSource = colList
        /// cboCurrency.DataBind()
        /// If Null.IsNull(objPortal.Currency) Or cboCurrency.Items.FindByValue(objPortal.Currency) Is Nothing Then
        ///     cboCurrency.Items.FindByValue("USD").Selected = True
        /// Else
        ///     cboCurrency.Items.FindByValue(objPortal.Currency).Selected = True
        /// End If
        /// </code></example></remarks>
        [XmlElement("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets default language for the portal.
        /// </summary>
        /// <value>Default language of the portal.</value>
        /// <returns>Default language of the portal.</returns>
        /// <remarks></remarks>
        [XmlElement("defaultlanguage")]
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Gets or sets description of the portal.
        /// </summary>
        /// <value>Description of the portal.</value>
        /// <returns>Description of the portal.</returns>
        /// <remarks><example>This show the usage of the <c>Description</c> property
        /// <code lang="vbnet">
        /// Dim objPortalController As New PortalController
        /// Dim objPortal As PortalInfo = objPortalController.GetPortal(PortalID)
        ///      txtPortalName.Text = objPortal.PortalName
        ///      txtDescription.Text = objPortal.Description
        ///      txtKeyWords.Text = objPortal.KeyWords
        ///  </code></example></remarks>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the default e-mail to be used in the porta.
        /// </summary>
        /// <value>E-mail of the portal.</value>
        /// <returns>E-mail of the portal.</returns>
        /// <remarks></remarks>
        [XmlElement("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets date at which the portal expires.
        /// </summary>
        /// <value>Date of expiration of the portal.</value>
        /// <returns>Date of expiration of the portal.</returns>
        /// <remarks><example>This show the Portal expiration date usage
        /// <code lang="vbnet">
        /// If Not Null.IsNull(objPortal.ExpiryDate) Then
        ///     txtExpiryDate.Text = objPortal.ExpiryDate.ToShortDateString
        /// End If
        /// txtHostFee.Text = objPortal.HostFee.ToString
        /// txtHostSpace.Text = objPortal.HostSpace.ToString
        /// txtPageQuota.Text = objPortal.PageQuota.ToString
        /// txtUserQuota.Text = objPortal.UserQuota.ToString
        /// </code></example></remarks>
        [XmlElement("expirydate")]
        public DateTime ExpiryDate { get; set; }


        /// <summary>
        /// Gets or sets gUID of the portal info object.
        /// </summary>
        /// <value>Portal info Object GUID.</value>
        /// <returns>GUD of the portal info object.</returns>
        /// <remarks></remarks>
        [XmlIgnore]
        public Guid GUID { get; set; }


        /// <summary>
        /// Gets or sets tabdId of the Home page.
        /// </summary>
        /// <value>TabId of the Home page.</value>
        /// <returns>TabId of the Home page.</returns>
        /// <remarks></remarks>
        [XmlElement("hometabid")]
        public int HomeTabId { get; set; }

        /// <summary>
        /// Gets or sets amount of currency that is used as a hosting fee of the portal.
        /// </summary>
        /// <value>Currency amount hosting fee.</value>
        /// <returns>Currency amount hosting fee.</returns>
        /// <remarks><example>This show the Portal <c>HostFee</c>usage
        /// <code lang="vbnet">
        /// If Not Null.IsNull(objPortal.ExpiryDate) Then
        ///     txtExpiryDate.Text = objPortal.ExpiryDate.ToShortDateString
        /// End If
        /// txtHostFee.Text = objPortal.HostFee.ToString
        /// txtHostSpace.Text = objPortal.HostSpace.ToString
        /// txtPageQuota.Text = objPortal.PageQuota.ToString
        /// txtUserQuota.Text = objPortal.UserQuota.ToString
        /// </code></example></remarks>
        [XmlElement("hostfee")]
        public float HostFee { get; set; }

        /// <summary>
        /// Gets or sets total disk space allowed for the portal (Mb). 0 means not limited.
        /// </summary>
        /// <value>Diskspace allowed for the portal.</value>
        /// <returns>Diskspace allowed for the portal.</returns>
        /// <remarks><example>This show the Portal <c>HostSpace</c>usage
        /// <code lang="vbnet">
        /// If Not Null.IsNull(objPortal.ExpiryDate) Then
        ///     txtExpiryDate.Text = objPortal.ExpiryDate.ToShortDateString
        /// End If
        /// txtHostFee.Text = objPortal.HostFee.ToString
        /// txtHostSpace.Text = objPortal.HostSpace.ToString
        /// txtPageQuota.Text = objPortal.PageQuota.ToString
        /// txtUserQuota.Text = objPortal.UserQuota.ToString
        /// </code></example></remarks>
        [XmlElement("hostspace")]
        public int HostSpace { get; set; }

        /// <summary>
        /// Gets or sets keywords (separated by ,) for this portal.
        /// </summary>
        /// <value>Keywords seperated by .</value>
        /// <returns>Keywords for this portal.</returns>
        /// <remarks><example>This show the usage of the <c>KeyWords</c> property
        /// <code lang="vbnet">
        /// Dim objPortalController As New PortalController
        /// Dim objPortal As PortalInfo = objPortalController.GetPortal(PortalID)
        ///      txtPortalName.Text = objPortal.PortalName
        ///      txtDescription.Text = objPortal.Description
        ///      txtKeyWords.Text = objPortal.KeyWords
        ///  </code></example></remarks>
        [XmlElement("keywords")]
        public string KeyWords { get; set; }

        /// <summary>
        /// Gets or sets tabId with the login control, page to login.
        /// </summary>
        /// <value>TabId of the Login page.</value>
        /// <returns>TabId of the Login page.</returns>
        /// <remarks></remarks>
        [XmlElement("logintabid")]
        public int LoginTabId { get; set; }

        /// <summary>
        /// Gets or sets the portal has a logo (bitmap) associated with the portal. Teh admin can set the logo in the portal settings.
        /// </summary>
        /// <value>URL of the logo.</value>
        /// <returns>URL of the Portal logo.</returns>
        /// <remarks><example><code lang="vbnet">
        ///  urlLogo.Url = objPortal.LogoFile
        ///  urlLogo.FileFilter = glbImageFileTypes
        /// </code></example></remarks>
        [XmlElement("logofile")]
        public string LogoFile { get; set; }

        /// <summary>
        /// Gets or sets number of portal pages allowed in the portal. 0 means not limited.
        /// </summary>
        /// <value>Number of portal pages allowed.</value>
        /// <returns>Number of portal pages allowed.</returns>
        /// <remarks><example>This show the Portal <c>PageQuota</c>usage
        /// <code lang="vbnet">
        /// If Not Null.IsNull(objPortal.ExpiryDate) Then
        ///     txtExpiryDate.Text = objPortal.ExpiryDate.ToShortDateString
        /// End If
        /// txtHostFee.Text = objPortal.HostFee.ToString
        /// txtHostSpace.Text = objPortal.HostSpace.ToString
        /// txtPageQuota.Text = objPortal.PageQuota.ToString
        /// txtUserQuota.Text = objPortal.UserQuota.ToString
        /// </code></example></remarks>
        [XmlElement("pagequota")]
        public int PageQuota { get; set; }

        /// <summary>
        /// Gets or sets name of the Payment processor that is used for portal payments, e.g. PayPal.
        /// </summary>
        /// <value>Name of the portal payment processor.</value>
        /// <returns>Name of the portal payment processor.</returns>
        /// <remarks></remarks>
        [XmlElement("paymentprocessor")]
        public string PaymentProcessor { get; set; }

        /// <summary>
        /// Gets or sets unique idenitifier of the Portal within the site.
        /// </summary>
        /// <value>Portal identifier.</value>
        /// <returns>Portal Identifier.</returns>
        /// <remarks></remarks>
        [XmlElement("portalid")]
        public int PortalID { get; set; }

        /// <summary>
        /// Gets or sets contains the id of the portal group that the portal belongs to
        /// Will be null or -1 (null.nullinteger) if the portal does not belong to a portal group.
        /// </summary>
        /// <value>Portal Group identifier.</value>
        /// <returns>Portal Group Identifier.</returns>
        /// <remarks></remarks>
        public int PortalGroupID { get; set; }

        /// <summary>
        /// Gets or sets name of the portal. Can be set at creation time, Admin can change the name in the portal settings.
        /// </summary>
        /// <value>Name of the portal.</value>
        /// <returns>Name of the portal.</returns>
        /// <remarks><example>This show the usage of the <c>PortalName</c> property
        /// <code lang="vbnet">
        /// Dim objPortalController As New PortalController
        /// Dim objPortal As PortalInfo = objPortalController.GetPortal(PortalID)
        ///      txtPortalName.Text = objPortal.PortalName
        ///      txtDescription.Text = objPortal.Description
        ///      txtKeyWords.Text = objPortal.KeyWords
        ///  </code></example></remarks>
        [XmlElement("portalname")]
        public string PortalName { get; set; }

        /// <summary>
        /// Gets or sets password to use in the payment processor.
        /// </summary>
        /// <value>Payment Processor password.</value>
        /// <returns></returns>
        /// <remarks><example>This shows the usage of the payment processing
        /// <code lang="vbnet">
        /// If objPortal.PaymentProcessor &lt;&gt; "" Then
        ///     If Not cboProcessor.Items.FindByText(objPortal.PaymentProcessor) Is Nothing Then
        ///         cboProcessor.Items.FindByText(objPortal.PaymentProcessor).Selected = True
        ///     Else       ' default
        ///          If Not cboProcessor.Items.FindByText("PayPal") Is Nothing Then
        ///                cboProcessor.Items.FindByText("PayPal").Selected = True
        ///           End If
        ///      End If
        ///      Else
        ///      cboProcessor.Items.FindByValue("").Selected = True
        /// End If
        /// txtUserId.Text = objPortal.ProcessorUserId
        /// txtPassword.Attributes.Add("value", objPortal.ProcessorPassword)
        /// </code></example></remarks>
        [XmlElement("processorpassword")]
        public string ProcessorPassword { get; set; }

        /// <summary>
        /// Gets or sets payment Processor userId.
        /// </summary>
        /// <value>
        /// <placeholder>Payment Processor userId</placeholder>
        /// </value>
        /// <returns></returns>
        /// <remarks> <seealso cref="PaymentProcessor"></seealso>
        /// <example>This shows the usage of the payment processing
        /// <code lang="vbnet">
        /// If objPortal.PaymentProcessor &lt;&gt; "" Then
        ///     If Not cboProcessor.Items.FindByText(objPortal.PaymentProcessor) Is Nothing Then
        ///         cboProcessor.Items.FindByText(objPortal.PaymentProcessor).Selected = True
        ///     Else       ' default
        ///          If Not cboProcessor.Items.FindByText("PayPal") Is Nothing Then
        ///                cboProcessor.Items.FindByText("PayPal").Selected = True
        ///           End If
        ///      End If
        ///      Else
        ///      cboProcessor.Items.FindByValue("").Selected = True
        /// End If
        /// txtUserId.Text = objPortal.ProcessorUserId
        /// txtPassword.Attributes.Add("value", objPortal.ProcessorPassword)
        /// </code></example></remarks>
        [XmlElement("processoruserid")]
        public string ProcessorUserId { get; set; }

        /// <summary>
        /// Gets or sets the RoleId of the Registered users group of the portal.
        /// </summary>
        /// <value>RoleId of the Registered users. </value>
        /// <returns>RoleId of the Registered users. </returns>
        /// <remarks></remarks>
        [XmlElement("registeredroleid")]
        public int RegisteredRoleId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Registration page.
        /// </summary>
        /// <value>TabId of the Registration page.</value>
        /// <returns>TabId of the Registration page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("registertabid")]
        public int RegisterTabId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Search profile page.
        /// </summary>
        /// <value>TabdId of the Search Results page.</value>
        /// <returns>TabdId of the Search Results page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("searchtabid")]
        public int SearchTabId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Custom 404 page.
        /// </summary>
        /// <value>Tabid of the Custom 404 page.</value>
        /// <returns>Tabid of the Custom 404 page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("custom404tabid")]
        public int Custom404TabId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Custom 500 error page.
        /// </summary>
        /// <value>Tabid of the Custom 500 error page.</value>
        /// <returns>Tabid of the Custom 500 error page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("custom500tabid")]
        public int Custom500TabId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Terms of Use page.
        /// </summary>
        /// <value>Tabid of the Terms of Use page.</value>
        /// <returns>Tabid of the Terms of Use page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("termstabid")]
        public int TermsTabId { get; set; }

        /// <summary>
        ///   Gets or sets tabid of the Privacy Statement page.
        /// </summary>
        /// <value>Tabid of the Privacy Statement page.</value>
        /// <returns>Tabid of the Privacy Statement page.</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("privacytabid")]
        public int PrivacyTabId { get; set; }

        /// <summary>
        /// Gets or sets # of days that Site log history should be kept. 0 means unlimited.
        /// </summary>
        /// <value># of days sitelog history.</value>
        /// <returns># of days sitelog history.</returns>
        [XmlElement("siteloghistory")]
        [Obsolete("Deprecated in 8.0.0. Scheduled removal in v11.0.0.")]
        public int SiteLogHistory { get; set; }

        /// <summary>
        /// Gets or sets tabdId of the splash page. If 0, there is no splash page.
        /// </summary>
        /// <value>TabdId of the Splash page.</value>
        /// <returns>TabdId of the Splash page.</returns>
        /// <remarks></remarks>
        [XmlElement("splashtabid")]
        public int SplashTabId { get; set; }

        /// <summary>
        /// Gets or sets tabId at which Host tasks start.
        /// </summary>
        /// <value>TabId of Host tasks.</value>
        /// <returns>TabId of Host tasks.</returns>
        /// <remarks></remarks>
        [XmlElement("supertabid")]
        public int SuperTabId { get; set; }

        /// <summary>
        /// Gets or sets number of registered users allowed in the portal. 0 means not limited.
        /// </summary>
        /// <value>Number of registered users allowed. </value>
        /// <returns>Number of registered users allowed. </returns>
        /// <remarks><example>This show the Portal userQuota usage
        /// <code lang="vbnet">
        /// If Not Null.IsNull(objPortal.ExpiryDate) Then
        ///     txtExpiryDate.Text = objPortal.ExpiryDate.ToShortDateString
        /// End If
        /// txtHostFee.Text = objPortal.HostFee.ToString
        /// txtHostSpace.Text = objPortal.HostSpace.ToString
        /// txtPageQuota.Text = objPortal.PageQuota.ToString
        /// txtUserQuota.Text = objPortal.UserQuota.ToString
        /// </code></example></remarks>
        [XmlElement("userquota")]
        public int UserQuota { get; set; }

        /// <summary>
        /// Gets or sets type of registration that the portal supports.
        /// </summary>
        /// <value>Type of registration.</value>
        /// <returns>Type of registration.</returns>
        /// <remarks><example>Registration type
        /// <code lang="vbnet">
        /// optUserRegistration.SelectedIndex = objPortal.UserRegistration
        /// </code></example></remarks>
        [XmlElement("userregistration")]
        public int UserRegistration { get; set; }

        /// <summary>
        /// Gets or sets tabid of the User profile page.
        /// </summary>
        /// <value>TabdId of the User profile page.</value>
        /// <returns>TabdId of the User profile page.</returns>
        /// <remarks></remarks>
        [XmlElement("usertabid")]
        public int UserTabId { get; set; }

        /// <summary>
        /// Gets or sets actual number of actual users for this portal.
        /// </summary>
        /// <value>Number of users for the portal.</value>
        /// <returns>Number of users for the portal.</returns>
        /// <remarks></remarks>
        [XmlElement("users")]
        public int Users
        {
            get
            {
                if (this._users < 0)
                {
                    this._users = UserController.GetUserCountByPortal(this.PortalID);
                }

                return this._users;
            }

            set { this._users = value; }
        }

        /// <summary>
        /// Gets or sets dNN Version # of the portal installation.
        /// </summary>
        /// <value>Version # of the portal installation.</value>
        /// <returns>Version # of the portal installation.</returns>
        /// <remarks></remarks>
        [XmlElement("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the actual name of the Administrators group of the portal.
        /// This name is retrieved from the RoleController object.
        /// </summary>
        /// <value>The name of the Administrators group.</value>
        /// <returns>The name of the Administrators group.</returns>
        /// <remarks></remarks>
        [XmlElement("administratorrolename")]
        public string AdministratorRoleName
        {
            get
            {
                if (this._administratorRoleName == Null.NullString && this.AdministratorRoleId > Null.NullInteger)
                {
                    // Get Role Name
                    RoleInfo adminRole = RoleController.Instance.GetRole(this.PortalID, r => r.RoleID == this.AdministratorRoleId);
                    if (adminRole != null)
                    {
                        this._administratorRoleName = adminRole.RoleName;
                    }
                }

                return this._administratorRoleName;
            }

            set
            {
                this._administratorRoleName = value;
            }
        }

        /// <summary>
        /// Gets or sets actual number of pages of the portal.
        /// </summary>
        /// <value>Number of pages of the portal.</value>
        /// <returns>Number of pages of the portal.</returns>
        /// <remarks></remarks>
        [XmlElement("pages")]
        public int Pages
        {
            get
            {
                if (this._pages < 0)
                {
                    this._pages = TabController.Instance.GetUserTabsByPortal(this.PortalID).Count;
                }

                return this._pages;
            }

            set
            {
                this._pages = value;
            }
        }

        /// <summary>
        /// Gets or sets the actual name of the Registerd Users group of the portal.
        /// This name is retrieved from the RoleController object.
        /// </summary>
        /// <value>The name of the Registerd Users group.</value>
        /// <returns>The name of the Registerd Users group.</returns>
        /// <remarks></remarks>
        [XmlElement("registeredrolename")]
        public string RegisteredRoleName
        {
            get
            {
                if (this._registeredRoleName == Null.NullString && this.RegisteredRoleId > Null.NullInteger)
                {
                    // Get Role Name
                    RoleInfo regUsersRole = RoleController.Instance.GetRole(this.PortalID, r => r.RoleID == this.RegisteredRoleId);
                    if (regUsersRole != null)
                    {
                        this._registeredRoleName = regUsersRole.RoleName;
                    }
                }

                return this._registeredRoleName;
            }

            set
            {
                this._registeredRoleName = value;
            }
        }

        [XmlIgnore]
        [Obsolete("Deprecated in DNN 6.0. Scheduled removal in v10.0.0.")]
        public int TimeZoneOffset { get; set; }

        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>KeyId of the IHydratable.Key.</returns>
        /// <remarks><seealso cref="Fill"></seealso></remarks>
        public int KeyID
        {
            get
            {
                return this.PortalID;
            }

            set
            {
                this.PortalID = value;
            }
        }

        /// <summary>
        /// Fills a PortalInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// <remarks>Standard IHydratable.Fill implementation.
        /// <seealso cref="KeyID"></seealso></remarks>
        public void Fill(IDataReader dr)
        {
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);

            try
            {
                this.PortalGroupID = Null.SetNullInteger(dr["PortalGroupID"]);
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
#pragma warning disable 612,618 //needed for upgrades and backwards compatibility
            this.TimeZoneOffset = Null.SetNullInteger(dr["TimeZoneOffset"]);
#pragma warning restore 612,618
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
