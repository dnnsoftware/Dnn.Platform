#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Entities.Portals
{
    /// <summary>
    /// PortalInfo provides a base class for Portal information
    /// This class inherites from the <c>BaseEntityInfo</c> and is <c>Hydratable</c>
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
		#region "Private Members"

        private string _administratorRoleName;
        private int _pages = Null.NullInteger;
        private string _registeredRoleName;
		
		#endregion

        #region Constructors

        /// <summary>
        /// Create new Portalinfo instance
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
            Users = Null.NullInteger;
        }

        #endregion

        #region Auto_Properties

        /// <summary>
        /// UserID of the user who is the admininistrator of the portal
        /// </summary>
        /// <value>UserId of the user who is the portal admin</value>
        /// <returns>UserId of the user who is the portal admin</returns>
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
        ///</code></example></remarks>
        [XmlElement("administratorid")]
        public int AdministratorId { get; set; }

        /// <summary>
        /// The RoleId of the Security Role of the Administrators group of the portal
        /// </summary>
        /// <value>RoleId of de Administrators Security Role</value>
        /// <returns>RoleId of de Administrators Security Role</returns>
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
        /// TabId at which admin tasks start
        /// </summary>
        /// <value>TabID of admin tasks</value>
        /// <returns>TabID of admin tasks</returns>
        /// <remarks></remarks>
        [XmlElement("admintabid")]
        public int AdminTabId { get; set; }

        /// <summary>
        /// Image (bitmap) file that is used as background for the portal
        /// </summary>
        /// <value>Name of the file that is used as background</value>
        /// <returns>Name of the file that is used as background</returns>
        /// <remarks></remarks>
        [XmlElement("backgroundfile")]
        public string BackgroundFile { get; set; }

        /// <summary>
        /// Setting for the type of banner advertising in the portal
        /// </summary>
        /// <value>Type of banner advertising</value>
        /// <returns>Type of banner advertising</returns>
        /// <remarks><example>This show the usage of BannerAdvertising setting
        /// <code lang="vbnet">
        /// optBanners.SelectedIndex = objPortal.BannerAdvertising
        /// </code></example></remarks>
        [XmlElement("banneradvertising")]
        public int BannerAdvertising { get; set; }

        [XmlElement("cultureCode")]
        public string CultureCode { get; set; }

        /// <summary>
        /// Curreny format that is used in the portal
        /// </summary>
        /// <value>Currency of the portal</value>
        /// <returns>Currency of the portal</returns>
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
        /// Default language for the portal
        /// </summary>
        /// <value>Default language of the portal</value>
        /// <returns>Default language of the portal</returns>
        /// <remarks></remarks>
        [XmlElement("defaultlanguage")]
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Description of the portal
        /// </summary>
        /// <value>Description of the portal</value>
        /// <returns>Description of the portal</returns>
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
        /// The default e-mail to be used in the porta;
        /// </summary>
        /// <value>E-mail of the portal</value>
        /// <returns>E-mail of the portal</returns>
        /// <remarks></remarks>
        [XmlElement("email")]
        public string Email { get; set; }

        /// <summary>
        /// Date at which the portal expires
        /// </summary>
        /// <value>Date of expiration of the portal</value>
        /// <returns>Date of expiration of the portal</returns>
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
        /// The footer text as specified in the Portal settings
        /// </summary>
        /// <value>Footer text of the portal</value>
        /// <returns>Returns the the footer text of the portal</returns>
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
        /// GUID of the portal info object
        /// </summary>
        /// <value>Portal info Object GUID</value>
        /// <returns>GUD of the portal info object</returns>
        /// <remarks></remarks>
        [XmlIgnore]
        public Guid GUID { get; set; }

        /// <summary>
        /// Home directory of the portal (logical path)
        /// </summary>
        /// <value>Portal home directory</value>
        /// <returns>Portal home directory</returns>
        /// <remarks><seealso cref="HomeDirectoryMapPath"></seealso></remarks>
        [XmlElement("homedirectory")]
        public string HomeDirectory { get; set; }

        /// <summary>
        /// Home System (local) directory of the portal (logical path)
        /// </summary>
        /// <value>Portal home system directory</value>
        /// <returns>Portal home system directory in local filesystem</returns>
        /// <remarks><seealso cref="HomeSystemDirectoryMapPath"></seealso></remarks>
        [XmlElement("homesystemdirectory")]
        public string HomeSystemDirectory {
            get { return String.Format("{0}-System", HomeDirectory); }
        }

        /// <summary>
        /// TabdId of the Home page
        /// </summary>
        /// <value>TabId of the Home page</value>
        /// <returns>TabId of the Home page</returns>
        /// <remarks></remarks>
        [XmlElement("hometabid")]
        public int HomeTabId { get; set; }

        /// <summary>
        /// Amount of currency that is used as a hosting fee of the portal
        /// </summary>
        /// <value>Currency amount hosting fee</value>
        /// <returns>Currency amount hosting fee</returns>
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
        /// Total disk space allowed for the portal (Mb). 0 means not limited
        /// </summary>
        /// <value>Diskspace allowed for the portal</value>
        /// <returns>Diskspace allowed for the portal</returns>
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
        /// Keywords (separated by ,) for this portal
        /// </summary>
        /// <value>Keywords seperated by ,</value>
        /// <returns>Keywords for this portal</returns>
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
        /// TabId with the login control, page to login
        /// </summary>
        /// <value>TabId of the Login page</value>
        /// <returns>TabId of the Login page</returns>
        /// <remarks></remarks>
        [XmlElement("logintabid")]
        public int LoginTabId { get; set; }

        /// <summary>
        /// The portal has a logo (bitmap) associated with the portal. Teh admin can set the logo in the portal settings
        /// </summary>
        /// <value>URL of the logo</value>
        /// <returns>URL of the Portal logo</returns>
        /// <remarks><example><code lang="vbnet">
        ///  urlLogo.Url = objPortal.LogoFile
        ///  urlLogo.FileFilter = glbImageFileTypes
        ///</code></example></remarks>
        [XmlElement("logofile")]
        public string LogoFile { get; set; }

        /// <summary>
        /// Number of portal pages allowed in the portal. 0 means not limited
        /// </summary>
        /// <value>Number of portal pages allowed</value>
        /// <returns>Number of portal pages allowed</returns>
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
        /// Name of the Payment processor that is used for portal payments, e.g. PayPal
        /// </summary>
        /// <value>Name of the portal payment processor</value>
        /// <returns>Name of the portal payment processor</returns>
        /// <remarks></remarks>
        [XmlElement("paymentprocessor")]
        public string PaymentProcessor { get; set; }

        /// <summary>
        /// Unique idenitifier of the Portal within the site
        /// </summary>
        /// <value>Portal identifier</value>
        /// <returns>Portal Identifier</returns>
        /// <remarks></remarks>
        [XmlElement("portalid")]
        public int PortalID { get; set; }

        /// <summary>
        /// Contains the id of the portal group that the portal belongs to
        /// Will be null or -1 (null.nullinteger) if the portal does not belong to a portal group
        /// </summary>
        /// <value>Portal Group identifier</value>
        /// <returns>Portal Group Identifier</returns>
        /// <remarks></remarks>
        public int PortalGroupID { get; set; }

        /// <summary>
        /// Name of the portal. Can be set at creation time, Admin can change the name in the portal settings
        /// </summary>
        /// <value>Name of the portal</value>
        /// <returns>Name of the portal</returns>
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
        /// Password to use in the payment processor
        /// </summary>
        /// <value>Payment Processor password</value>
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
        /// Payment Processor userId
        /// </summary>
        /// <value></value>
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
        /// The RoleId of the Registered users group of the portal.
        /// </summary>
        /// <value>RoleId of the Registered users </value>
        /// <returns>RoleId of the Registered users </returns>
        /// <remarks></remarks>
        [XmlElement("registeredroleid")]
        public int RegisteredRoleId { get; set; }

        /// <summary>
        ///   Tabid of the Registration page
        /// </summary>
        /// <value>TabId of the Registration page</value>
        /// <returns>TabId of the Registration page</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("registertabid")]
        public int RegisterTabId { get; set; }

        /// <summary>
        ///   Tabid of the Search profile page
        /// </summary>
        /// <value>TabdId of the Search Results page</value>
        /// <returns>TabdId of the Search Results page</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("searchtabid")]
        public int SearchTabId { get; set; }

        /// <summary>
        ///   Tabid of the Custom 404 page
        /// </summary>
        /// <value>Tabid of the Custom 404 page</value>
        /// <returns>Tabid of the Custom 404 page</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("custom404tabid")]
        public int Custom404TabId { get; set; }

        /// <summary>
        ///   Tabid of the Custom 500 error page
        /// </summary>
        /// <value>Tabid of the Custom 500 error page</value>
        /// <returns>Tabid of the Custom 500 error page</returns>
        /// <remarks>
        /// </remarks>
        [XmlElement("custom500tabid")]
        public int Custom500TabId { get; set; }

        /// <summary>
        /// # of days that Site log history should be kept. 0 means unlimited
        /// </summary>
        /// <value># of days sitelog history</value>
        /// <returns># of days sitelog history</returns>
        [XmlElement("siteloghistory")]
        [Obsolete("Deprecated in 8.0.0")]
        public int SiteLogHistory { get; set; }

        /// <summary>
        /// TabdId of the splash page. If 0, there is no splash page
        /// </summary>
        /// <value>TabdId of the Splash page</value>
        /// <returns>TabdId of the Splash page</returns>
        /// <remarks></remarks>
        [XmlElement("splashtabid")]
        public int SplashTabId { get; set; }

        /// <summary>
        /// TabId at which Host tasks start
        /// </summary>
        /// <value>TabId of Host tasks</value>
        /// <returns>TabId of Host tasks</returns>
        /// <remarks></remarks>
        [XmlElement("supertabid")]
        public int SuperTabId { get; set; }

        /// <summary>
        /// Number of registered users allowed in the portal. 0 means not limited
        /// </summary>
        /// <value>Number of registered users allowed </value>
        /// <returns>Number of registered users allowed </returns>
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
        /// Type of registration that the portal supports
        /// </summary>
        /// <value>Type of registration</value>
        /// <returns>Type of registration</returns>
        /// <remarks><example>Registration type
        /// <code lang="vbnet">
        /// optUserRegistration.SelectedIndex = objPortal.UserRegistration
        /// </code></example></remarks>
        [XmlElement("userregistration")]
        public int UserRegistration { get; set; }

        /// <summary>
        /// Tabid of the User profile page
        /// </summary>
        /// <value>TabdId of the User profile page</value>
        /// <returns>TabdId of the User profile page</returns>
        /// <remarks></remarks>
        [XmlElement("usertabid")]
        public int UserTabId { get; set; }

        private int _users;

        /// <summary>
        /// Actual number of actual users for this portal
        /// </summary>
        /// <value>Number of users for the portal</value>
        /// <returns>Number of users for the portal</returns>
        /// <remarks></remarks>
        [XmlElement("users")]
        public int Users
        {
            get
            {
                if (_users < 0)
                {
                    _users = UserController.GetUserCountByPortal(PortalID);
                }
                return _users;
            }
            set { _users = value; }
        }

        /// <summary>
        /// DNN Version # of the portal installation
        /// </summary>
        /// <value>Version # of the portal installation</value>
        /// <returns>Version # of the portal installation</returns>
        /// <remarks></remarks>
        [XmlElement("version")]
        public string Version { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The actual name of the Administrators group of the portal.
        /// This name is retrieved from the RoleController object
        /// </summary>
        /// <value>The name of the Administrators group</value>
        /// <returns>The name of the Administrators group</returns>
        /// <remarks></remarks>
        [XmlElement("administratorrolename")]
        public string AdministratorRoleName
        {
            get
            {
                if (_administratorRoleName == Null.NullString && AdministratorRoleId > Null.NullInteger)
                {
					//Get Role Name
                    RoleInfo adminRole = RoleController.Instance.GetRole(PortalID, r => r.RoleID == AdministratorRoleId);
                    if (adminRole != null)
                    {
                        _administratorRoleName = adminRole.RoleName;
                    }
                }
                return _administratorRoleName;
            }
            set
            {
                _administratorRoleName = value;
            }
        }

        /// <summary>
        /// Fysical path on disk of the home directory of the portal
        /// </summary>
        /// <value></value>
        /// <returns>Fully qualified path of the home directory</returns>
        /// <remarks><seealso cref="HomeDirectory"></seealso></remarks>
        [XmlIgnore]
        public string HomeDirectoryMapPath
        {
            get
            {
                return String.Format("{0}\\{1}\\", Globals.ApplicationMapPath, HomeDirectory.Replace("/", "\\"));
            }
        }

        /// <summary>
        /// Fysical path on disk of the home directory of the portal
        /// </summary>
        /// <value></value>
        /// <returns>Fully qualified path of the home system (local) directory</returns>
        /// <remarks><seealso cref="HomeDirectory"></seealso></remarks>
        [XmlIgnore]
        public string HomeSystemDirectoryMapPath
        {
            get
            {
                return String.Format("{0}\\{1}\\", Globals.ApplicationMapPath, HomeSystemDirectory.Replace("/", "\\"));
            }
        }

        /// <summary>
        /// Actual number of pages of the portal
        /// </summary>
        /// <value>Number of pages of the portal</value>
        /// <returns>Number of pages of the portal</returns>
        /// <remarks></remarks>
        [XmlElement("pages")]
        public int Pages
        {
            get
            {
                if (_pages < 0)
                {
                    _pages = TabController.Instance.GetTabsByPortal(PortalID).Count;
                }
                return _pages;
            }
            set
            {
                _pages = value;
            }
        }

        /// <summary>
        /// The actual name of the Registerd Users group of the portal.
        /// This name is retrieved from the RoleController object
        /// </summary>
        /// <value>The name of the Registerd Users group</value>
        /// <returns>The name of the Registerd Users group</returns>
        /// <remarks></remarks>
        [XmlElement("registeredrolename")]
        public string RegisteredRoleName
        {
            get
            {
                if (_registeredRoleName == Null.NullString && RegisteredRoleId > Null.NullInteger)
                {
					//Get Role Name
                    RoleInfo regUsersRole = RoleController.Instance.GetRole(PortalID, r => r.RoleID == RegisteredRoleId);
                    if (regUsersRole != null)
                    {
                        _registeredRoleName = regUsersRole.RoleName;
                    }
                }
                return _registeredRoleName;
            }
            set
            {
                _registeredRoleName = value;
            }
        }

        #endregion

        [XmlIgnore]
        [Obsolete("Deprecated in DNN 6.0.")]
        public int TimeZoneOffset { get; set; }

        #region IHydratable Members

        /// <summary>
        /// Fills a PortalInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// <remarks>Standard IHydratable.Fill implementation
        /// <seealso cref="KeyID"></seealso></remarks>
        public void Fill(IDataReader dr)
        {
            PortalID = Null.SetNullInteger(dr["PortalID"]);

            try
            {
                PortalGroupID = Null.SetNullInteger(dr["PortalGroupID"]);
            }
            catch (IndexOutOfRangeException)
            {
                if(Globals.Status == Globals.UpgradeStatus.None)
                {
                    //this should not happen outside of an upgrade situation
                    throw;
                }

                //else swallow the error
            }
            
            PortalName = Null.SetNullString(dr["PortalName"]);
            LogoFile = Null.SetNullString(dr["LogoFile"]);
            FooterText = Null.SetNullString(dr["FooterText"]);
            ExpiryDate = Null.SetNullDateTime(dr["ExpiryDate"]);
            UserRegistration = Null.SetNullInteger(dr["UserRegistration"]);
            BannerAdvertising = Null.SetNullInteger(dr["BannerAdvertising"]);
            AdministratorId = Null.SetNullInteger(dr["AdministratorID"]);
            Email = Null.SetNullString(dr["Email"]);
            Currency = Null.SetNullString(dr["Currency"]);
            HostFee = Null.SetNullInteger(dr["HostFee"]);
            HostSpace = Null.SetNullInteger(dr["HostSpace"]);
            PageQuota = Null.SetNullInteger(dr["PageQuota"]);
            UserQuota = Null.SetNullInteger(dr["UserQuota"]);
            AdministratorRoleId = Null.SetNullInteger(dr["AdministratorRoleID"]);
            RegisteredRoleId = Null.SetNullInteger(dr["RegisteredRoleID"]);
            Description = Null.SetNullString(dr["Description"]);
            KeyWords = Null.SetNullString(dr["KeyWords"]);
            BackgroundFile = Null.SetNullString(dr["BackGroundFile"]);
            GUID = new Guid(Null.SetNullString(dr["GUID"]));
            PaymentProcessor = Null.SetNullString(dr["PaymentProcessor"]);
            ProcessorUserId = Null.SetNullString(dr["ProcessorUserId"]);
            var p = Null.SetNullString(dr["ProcessorPassword"]);
            try
            {
                ProcessorPassword = string.IsNullOrEmpty(p)
                    ? p
                    : Security.FIPSCompliant.DecryptAES(p, Config.GetDecryptionkey(), Host.Host.GUID);
            }
            catch(FormatException)
            {
                // for backward compatibility
                ProcessorPassword = p;
            }
            SplashTabId = Null.SetNullInteger(dr["SplashTabID"]);
            HomeTabId = Null.SetNullInteger(dr["HomeTabID"]);
            LoginTabId = Null.SetNullInteger(dr["LoginTabID"]);
            RegisterTabId = Null.SetNullInteger(dr["RegisterTabID"]);
            UserTabId = Null.SetNullInteger(dr["UserTabID"]);
            SearchTabId = Null.SetNullInteger(dr["SearchTabID"]);

            Custom404TabId = Custom500TabId = Null.NullInteger;
            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'Custom404TabId'").Length > 0)
                {
                    Custom404TabId = Null.SetNullInteger(dr["Custom404TabId"]);
                }
                if (schema.Select("ColumnName = 'Custom500TabId'").Length > 0)
                {
                    Custom500TabId = Null.SetNullInteger(dr["Custom500TabId"]);
                }
            }

            DefaultLanguage = Null.SetNullString(dr["DefaultLanguage"]);
#pragma warning disable 612,618 //needed for upgrades and backwards compatibility
            TimeZoneOffset = Null.SetNullInteger(dr["TimeZoneOffset"]);
#pragma warning restore 612,618
            AdminTabId = Null.SetNullInteger(dr["AdminTabID"]);
            HomeDirectory = Null.SetNullString(dr["HomeDirectory"]);
            SuperTabId = Null.SetNullInteger(dr["SuperTabId"]);
            CultureCode = Null.SetNullString(dr["CultureCode"]);

            FillInternal(dr);
            AdministratorRoleName = Null.NullString;
            RegisteredRoleName = Null.NullString;

            Users = Null.NullInteger;
            Pages = Null.NullInteger;
        }

        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>KeyId of the IHydratable.Key</returns>
        /// <remarks><seealso cref="Fill"></seealso></remarks>
        public int KeyID
        {
            get
            {
                return PortalID;
            }
            set
            {
                PortalID = value;
            }
        }

        #endregion
    }
}
