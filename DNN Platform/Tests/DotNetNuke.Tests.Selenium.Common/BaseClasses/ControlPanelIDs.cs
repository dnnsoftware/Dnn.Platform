using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace DNNSelenium.Common.BaseClasses
{
	public class ControlPanelIDs
	{
		public static string CompanyLogo = "//a/img[@id = 'dnn_dnnLOGO_imgLogo']";
		public static string AwesomeCycles = "Awesome-Cycles-Logo.png";
		public static string AnovaMobility = "logo_Anova.png";

		public static string PageTitleID = "//span[contains(@id, 'dnnTITLE_titleLabel')]";
		public static string PageHeaderID = "//span[@id= 'dnn_dnnBreadcrumb_lblBreadCrumb']/a[last()]";

		public static string RegisterLink = "//a[@id='dnn_dnnUser_enhancedRegisterLink']";
		public static string LoginLink = "//*[@id='dnn_dnnLogin_enhancedLoginLink' and not(contains(@href, 'Logoff'))]";
		public static string LogoutLinkID = "dnn_dnnLogin_enhancedLoginLink";
		public static string LogoutLink = "//*[@id='" + LogoutLinkID + "' and contains(@href, 'Logoff')]";

		public static string SocialRegisterLink = "//a[@id='dnn_userLogin_registerLink']";
		public static string SocialLoginLink = "//a[@id='dnn_userLogin_loginLink']";
		public static string SocialUserLink = "//a[@id = 'dnn_dnnUser_userNameLink']";

		public static string SocialUserMenu = "//ul[@class = 'userMenu']";
		public static string SocialLogoutLink = "//a[@id = 'dnn_userLogin_logoffLink']";
		public static string SocialMyProfileLink = "//a[@id = 'dnn_userLogin_viewProfileLink']";
		public static string SocialEditProfile = "//a[@id = 'dnn_userLogin_editProfileLink']";
		public static string SocialEditAccount = "//a[@id = 'dnn_userLogin_accountLink']";

		public static string MessageLink = "dnn_dnnUser_messageLink";
		public static string NotificationLink = "dnn_dnnUser_notificationLink";
		public static string UserAvatar = "dnn_dnnUser_avatar";

		public static string SearchBox = "//input[contains(@id,'dnnSearch_txtSearch')]";
		public static string SearchButton = "//a[contains(@id, 'dnnSearch_cmdSearch')]";

		public static string LanguageIcon = "//span[contains(@class, 'Language')]";

		public static string CopyrightNotice = "dnn_dnnCopyright_lblCopyright";
		public static string CopyrightText = "Copyright 2013 by DNN Corp";

		#region Control Panel ID's

		public static string ControlPanelAdminOption = "//ul[@id='ControlNav']/li[1]/a";
		public static string ControlPanelHostOption = "//ul[@id='ControlNav']/li[2]/a";
		public static string ControlPanelToolsOption = "//ul[@id='ControlNav']/li[3]/a";
		public static string ControlPanelHelpOption = "//ul[@id='ControlNav']/li[4]/a";
		public static string ControlPanelModulesOption = "//ul[@id='ControlActionMenu']/li[1]/a";
		public static string ControlPanelPagesOption = "//ul[@id='ControlActionMenu']/li[2]/a";
		public static string ControlPanelUsersOption = "//ul[@id='ControlActionMenu']/li[3]/a";
		public static string ControlPanelEditPageOption = "//ul[@id = 'ControlEditPageMenu']//a[span[@class = 'controlBar_editPageTxt']]";

		public static string ControlPanelAdminCommonSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_admin_basic']";
		public static string ControlPanelHostCommonSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_host_basic']";
		public static string ControlPanelHostAdvancedSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_host_advanced']";
		public static string ControlPanelAdminAdvancedSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_admin_advanced']";

		public static string AdminAdvancedSettingsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Advanced Settings']/a[1]";
		public static string AdminEventViewerOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Log Viewer']/a[1]";
		public static string AdminFileManagementOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='File Management']/a[1]";
		public static string AdminPageManagementOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Pages']/a[1]";
		public static string AdminRecycleBinOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Recycle Bin']/a[1]";
		public static string AdminSecurityRolesOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Security Roles']/a[1]";
		public static string AdminSiteSettingsOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Site Settings']/a[1]";
		public static string AdminUserAccountsOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='User Accounts']/a[1]";
		public static string AdminDevicePreviewManagementOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Device Preview Management']/a[1]";
		public static string AdminExtensionsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Extensions']/a[1]";
		public static string AdminLanguagesOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Languages']/a[1]";
		public static string AdminListsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Lists']/a[1]";
		public static string AdminNewslettersOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Newsletters']/a[1]";
		public static string AdminSearchAdminOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Search Admin']/a[1]";
		public static string AdminSearchEngineSiteMapOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Search Engine SiteMap']/a[1]";
		public static string AdminSiteLogOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Log']/a[1]";
		public static string AdminSiteRedirectionManagementOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Redirection Management']/a[1]";
		public static string AdminSiteWizardOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Wizard']/a[1]";
		public static string AdminSkinsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Skins']/a[1]";
		public static string AdminTaxonomyOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Taxonomy']/a[1]";
		public static string AdminVendorsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Vendors']/a[1]";

		public static string HostDashboardOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Dashboard']/a[1]";
		public static string HostExtensionsOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Extensions']/a[1]";
		public static string HostFileManagementOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='File Management']/a[1]";
		public static string HostHostSettingsOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Host Settings']/a[1]";
		public static string HostSiteManagementOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Site Management']/a[1]";
		public static string HostConfigurationManagerOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Configuration Manager']/a[1]";
		public static string HostDeviceDetectionManagementOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Device Detection Management']/a[1]";
		public static string HostHtmlEditorManagerOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='HTML Editor Manager']/a[1]";
		public static string HostListsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Lists']/a[1]";
		public static string HostScheduleOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Schedule']/a[1]";
		public static string HostSearchAdminOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Search Admin']/a[1]";
		public static string HostSqlOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='SQL']/a[1]";
		public static string HostSuperuserAccountsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Superuser Accounts']/a[1]";
		public static string HostVendorsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Vendors']/a[1]";

		public static string ToolsGoButton = "//input[@id = 'controlBar_SwitchSiteButton']";
		public static string ToolsFileUploadOption = "//a[contains(text(), 'Upload File')]";

		public static string HelpGettingStartedOption = "//li[@id = 'ControlBar_gettingStartedLink']/a";

		public static string AddNewUserOption = "//ul[@id='ControlActionMenu']/li[3]//li[1]/a";
		public static string UsersManageUsersOption = "//ul[@id='ControlActionMenu']/li[3]//li[2]/a";
		public static string UsersManageRolesOption = "//ul[@id='ControlActionMenu']/li[3]//li[3]/a";

		public static string PagesAddNewPageOption = "//ul[@id='ControlActionMenu']/li[2]//li[1]/a";
		public static string PagesCopyPageOption = "//ul[@id='ControlActionMenu']/li[2]//li[2]/a";
		public static string PagesImportPageOption = "//ul[@id='ControlActionMenu']/li[2]//a[contains(@href, 'ImportTab')]";

		public static string ModulesAddNewModuleOption = "//a[@id = 'controlBar_AddNewModule']";
		public static string ModulesAddExistingModuleOption = "//a[@id = 'controlBar_AddExistingModule']";

		public static string EditThisPageButton = "//a[@id = 'ControlBar_EditPage' and contains(text(), 'Edit This Page')]";
		public static string CloseEditModeButton = "//a[@id = 'ControlBar_EditPage' and contains(text(), 'Close Edit Mode')]";
		public static string PageInEditMode = "//a[@class = 'controlBar_editPageInEditMode']";
		public static string PageSettingsOption = "//a[contains(@href, 'edit/activeTab/settingTab')]";
		public static string ExportPageOption = "//a[contains(@href, 'ctl/ExportTab')]";
		public static string DeletePageOption = "//a[@id = 'ControlBar_DeletePage']";

		#endregion
	}
}
