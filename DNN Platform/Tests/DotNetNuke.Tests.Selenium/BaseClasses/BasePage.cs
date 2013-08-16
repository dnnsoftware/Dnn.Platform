using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace DotNetNuke.Tests.DNNSelenium.BaseClasses
{
	public class BasePage
	{
		protected readonly IWebDriver _driver;

		public static string PageTitle = "dnnTITLE_titleLabel";

		public static string LogoutLink = "dnn_dnnLogin_enhancedLoginLink";

		public static string MessageLink = "dnn_dnnUser_messageLink";
		public static string NotificationLink = "dnn_dnnUser_notificationLink";
		public static string RegisteredUserLink = "dnn_dnnUser_enhancedRegisterLink";
		public static string UserAvatar = "dnn_dnnUser_avatar";

		public static string SearchBox = "//input[contains(@id,'dnnSearch_txtSearch')]";
		public static string SearchButton = "//a[contains(@id, 'dnnSearch_cmdSearch')]";
				
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
		public static string ControlPanelAdminAdvancedSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_admin_advanced']";
		public static string ControlPanelHostCommonSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_host_basic']";
		public static string ControlPanelHostAdvancedSettings = "//ul[@id='ControlNav']//a[@href = '#controlbar_host_advanced']";

		public static string AdminEventViewerOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Log Viewer']/a[1]";
		public static string AdminFileManagementOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='File Management']/a[1]";
		public static string AdminPageManagementOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Pages']/a[1]";
		public static string AdminRecycleBinOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Recycle Bin']/a[1]";
		public static string AdminSecurityRolesOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Security Roles']/a[1]";
		public static string AdminSiteSettingsOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='Site Settings']/a[1]";
		public static string AdminUserAccountsOption = "//dl[@id='controlbar_admin_basic']//li[@data-tabname='User Accounts']/a[1]";
		public static string AdminAdvancedSettingsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Advanced Settings']/a[1]";
		public static string AdminAdvancedUrlManagementOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Advanced URL Management']/a[1]";
		public static string AdminContentStagingOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Content Staging']/a[1]";
		public static string AdminDevicePreviewManagementOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Device Preview Management']/a[1]";
		public static string AdminExtensionsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Extensions']/a[1]";
		public static string GamingMechanicsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Gaming Mechanics']/a[1]";
		public static string AdminGoogleAnalyticsProOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Google Analytics Pro']/a[1]";
		public static string AdminGoogleAnalyticsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Google Analytics']/a[1]";
		public static string AdminLanguagesOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Languages']/a[1]";
		public static string AdminListsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Lists']/a[1]";
		public static string AdminNewslettersOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Newsletters']/a[1]";
		public static string AdminSearchAdminOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Search Admin']/a[1]";
		public static string AdminSearchEngineSiteMapOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Search Engine SiteMap']/a[1]";
		public static string AdminSharePointConnectorOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='SharePoint Connector']/a[1]";
		public static string AdminSiteLogOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Log']/a[1]";
		public static string AdminSiteRedirectionManagementOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Redirection Management']/a[1]";
		public static string AdminSiteWizardOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Site Wizard']/a[1]";
		public static string AdminSkinsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Skins']/a[1]";
		public static string AdminTaxonomyOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Taxonomy']/a[1]";
		public static string AdminVendorsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Vendors']/a[1]";

		public static string HostDashboardOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Dashboard']/a[1]";
		public static string HostExtensionsOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Extensions']/a[1]";
		public static string HostFileManagementOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='File Management']/a[1]";
		public static string HostHealthMonitoringOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Health Monitoring']/a[1]";
		public static string HostHostSettingsOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Host Settings']/a[1]";
		public static string HostKnowledgeBaseOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Knowledge Base']/a[1]";
		public static string HostSiteManagementOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Site Management']/a[1]";
		public static string HostSoftwareAndDocumentationOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Software and Documentation']/a[1]";
		public static string HostTechnicalSupportOption = "//dl[@id='controlbar_host_basic']//li[@data-tabname='Technical Support']/a[1]";
		public static string HostApplicationIntegrityOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Application Integrity']/a[1]";
		public static string HostActivateYourLicenseOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Activate Your License']/a[1]";
		public static string HostAdvancedUrlManagementOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Advanced URL Management']/a[1]";
		public static string HostConfigurationManagerOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Configuration Manager']/a[1]";
		public static string HostDeviceDetectionManagementOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Device Detection Management']/a[1]";
		public static string HostHtmlEditorManagerOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='HTML Editor Manager']/a[1]";
		public static string HostLicenseManagementOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='License Management']/a[1]";
		public static string HostListsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Lists']/a[1]";
		public static string HostManageWebServersOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Manage Web Servers']/a[1]";
		public static string HostMySupportTicketsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='My Support Tickets']/a[1]";
		public static string HostScheduleOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Schedule']/a[1]";
		public static string HostSearchAdminOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Search Admin']/a[1]";
		public static string HostSearchCrawlerAdminOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='SearchCrawler Admin']/a[1]";
		public static string HostSecurityCenterOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Security Center']/a[1]";
		public static string HostSharePointConnectorOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='SharePoint Connector']/a[1]";
		public static string HostSiteGroupsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Site Groups']/a[1]";
		public static string HostSqlOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='SQL']/a[1]";
		public static string HostSuperuserAccountsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Superuser Accounts']/a[1]";
		public static string HostUserSwitcherOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='User Switcher']/a[1]";
		public static string HostVendorsOption = "//dl[@id='controlbar_host_advanced']//li[@data-tabname='Vendors']/a[1]";
		public static string HostWhatsNewOption = "//dl[@id='controlbar_host_advanced']//li[contains(@data-tabname, 'What')]/a[1]";

		public static string ToolsGoButton = "//input[@id = 'controlBar_SwitchSiteButton']";
		public static string ToolsFileUploadOption = "//a[contains(text(), 'Upload File')]";

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
		public static string PageSettingsOption = "//a[contains(@href, 'edit/activeTab/settingTab')]";
		public static string ExportPageOption = "//a[contains(@href, 'ctl/ExportTab')]";
		public static string DeletePageOption = "//a[@id = 'ControlBar_DeletePage']";


		#endregion

		public const string TraceLevelLow = "\t\t\t[D] ";
		public const string TraceLevelElement = "\t\t[E] ";
		public const string TraceLevelPage = "\t[P] ";
		public const string TraceLevelComposite = "[C] ";
		public const string RunningTestKeyWord = "\r\nRunning TEST: ";
		public const string PreconditionsKeyWord = "Running Preconditions: ";

		private BasePage() {}
		public BasePage (IWebDriver driver)
		{
			_driver = driver;
		}

		public void GoToUrl (string url)
		{
			Trace.WriteLine(TraceLevelLow + "Open url: http://" + url);
			if (_driver != null) _driver.Navigate().GoToUrl("http://" + url);

			WaitTillPageIsLoaded(3 * 60);
		}

		protected Dictionary<string, string> _translate = null;
		public string Translate(string key)
		{
			string localized = _translate[key];
			Trace.WriteLine(BasePage.TraceLevelElement + "Translating '" + key + "'" + " to  '" + localized + "'");

			return localized;
		}

		public void SetPageToEditMode(string pageName)
		{
			if (ElementPresent(By.XPath(EditThisPageButton)))
			{
				Trace.WriteLine(TraceLevelPage + "Set the Page to Edit mode:");
				SelectMenuOption(ControlPanelEditPageOption, EditThisPageButton);
				WaitForElement(By.XPath("//a[@class = 'controlBar_editPageInEditMode']"));
			}
			else
			{
				Trace.WriteLine(TraceLevelPage + "The Page is already set to Edit mode");
			}
		}

		public void CloseEditMode(string pageName)
		{
			Trace.WriteLine(TraceLevelPage + "Set the Page to Edit mode:");
			SelectMenuOption(ControlPanelEditPageOption, CloseEditModeButton);
			FindElement(By.XPath("//a[@class = 'controlBar_editPageInEditMode']")).WaitTillNotVisible();
		}

		/*
			var js = (IJavaScriptExecutor)driver;
			js.ExecuteScript
			(
					"var scriptElement=document.createElement('script');" +
					"scriptElement.setAttribute('type','text/javascript');" +
					"document.getElementsByTagName('head')[0].appendChild(scriptElement);" +
					"scriptElement.appendChild(document.createTextNode('function myScript(){ alert(\\'my alert\\'); }'));"
			);

			js.ExecuteAsyncScript("myScript()");

			return;
*/
		#region Waitings
		
		public const int FindElementTimeOut = 15;
		public static IWebElement WaitForElement(IWebDriver driver, By locator, int timeOutSeconds = FindElementTimeOut)
		{
			timeOutSeconds *= Settings.Default.WaitFactor;

			Trace.Write(TraceLevelLow + "Wait for element '" + locator + "' is present. ");
			Trace.WriteLine("\r\n\t\t\tMax waiting time: " + timeOutSeconds + " sec");

			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOutSeconds));
			return wait.Until(d => d.FindElement(locator));
		}

		public IWebElement WaitForElement(By locator, int timeOutSeconds = FindElementTimeOut)
		{
			return WaitForElement(_driver, locator, timeOutSeconds);
		}

		public static void WaitForElementNotPresent(IWebDriver driver, By locator, int timeOutSeconds = FindElementTimeOut)
		{
			timeOutSeconds *= Settings.Default.WaitFactor;

			Trace.Write(TraceLevelLow + "Wait for element '" + locator + "' is NOT present. ");
			Trace.WriteLine("\r\n\t\t\tMax waiting time: " + timeOutSeconds + " sec");

			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOutSeconds));
			wait.Until(d =>
				           {
					           try
					           {
								   return d.FindElement(locator) == null;
							   }
							   catch (NoSuchElementException)
							   {
								   return true;
							   }
						   });
		}

		public void WaitForElementNotPresent(By locator, int timeOutSeconds = FindElementTimeOut)
		{
			WaitForElementNotPresent(_driver, locator, timeOutSeconds);
		}

		public static void WaitTillStopMoving (IWebDriver driver, IWebElement element, Point startPos, int timeOutSeconds = FindElementTimeOut)
		{
			timeOutSeconds *= Settings.Default.WaitFactor;

			DateTime startTime = DateTime.UtcNow;

			while (element.Location == startPos)
			{
				if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(timeOutSeconds))
					throw new TimeoutException("Timout while waiting for element to START moving.");

				//Trace.WriteLine("Waiting for move to start.");

				Thread.Sleep(100);
			}

			Point lastPos = startPos;
			do
			{
				if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(timeOutSeconds))
					throw new TimeoutException("Timout while waitng for element to STOP moving.");

				Point curPos = element.Location;
				//Trace.WriteLine("curPos = " + curPos);

				if (curPos == lastPos)
				{
					break;
				}

				lastPos = curPos;
				Thread.Sleep(100);
			}
			while (true);
		}

		public void WaitTillStopMoving(IWebElement element, Point startPos, int timeOutSeconds = FindElementTimeOut)
		{
			WaitTillStopMoving(_driver, element, startPos, timeOutSeconds);
		}

		public void WaitTillPageIsLoaded(int timeOutSeconds)
		{
			timeOutSeconds *= Settings.Default.WaitFactor;

			// Cycle for 5 minutes or until
			// FindElement stops throwing exceptions
			Trace.WriteLine(TraceLevelLow + "Max waiting time for page to load: " + timeOutSeconds + " sec");

			DateTime startTime = DateTime.UtcNow;
			do
			{
				try
				{
					//Trace.Write("Looking for //body... ");
					_driver.FindElement(By.XPath("//body"));
					break;
				}
				catch (Exception) { }

				Thread.Sleep(TimeSpan.FromSeconds(0.3));
			}
			while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(timeOutSeconds));
		}

		#endregion

		public IWebElement FindElement(By locator)
		{
			Trace.WriteLine(TraceLevelLow + "Looking for element: '" + locator + "'");
			return _driver.FindElement(locator);
		}

		public void Type(By locator, string value)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Type '" + value + "' in input field: '" + locator + "'");
			FindElement(locator).SendKeys(value);
		}

		public void WaitAndType(By locator, int timeOutSeconds, string value)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait and Type '" + value + "' in input field: '" + locator + "'");
			WaitForElement(locator, timeOutSeconds).SendKeys(value);
		}

		public void WaitAndType(By locator, string value)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait and Type '" + value + "' in input field: '" + locator + "'");

			WaitForElement(locator).SendKeys(value);
		}

		public void Clear(By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Find and Clear: '" + locator + "'");
			FindElement(locator).Clear();
		}

		public void Click(By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Find and Click: '" + locator + "'");
			FindElement(locator).Click();
		}

		public void WaitAndClick(By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait and Click: '" + locator + "'");
			WaitForElement(locator).Click();
		}

		public void WaitAndSwitchToFrame(int timeOutSeconds)
		{
			_driver.SwitchTo().DefaultContent();
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait for frame is present and switch to frame");
			_driver.SwitchTo().Frame(WaitForElement(By.Id("iPopUp"), timeOutSeconds));
		}

		public void WaitAndSwitchToWindow(int timeOutSeconds)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait for frame is NOT present and switch to window");
			WaitForElementNotPresent(By.Id("iPopUp"), timeOutSeconds);
			_driver.SwitchTo().DefaultContent();

			WaitForElement(By.Id(CopyrightNotice), timeOutSeconds).WaitTillVisible();
		}

		public void ClickCloseButtonOnFrame(int timeOutSeconds)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on 'Close' button on frame and switch to window");
			_driver.SwitchTo().DefaultContent();
			FindElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//a[@role='button'] ")).WaitTillVisible().Click();
			WaitForElement(By.Id(CopyrightNotice), timeOutSeconds).WaitTillVisible();
		}

		public string CurrentFrameTitle()
		{
			WaitForElement(By.XPath("//div[@aria-describedby = 'iPopUp']"));

			string _title = WaitForElement(By.XPath("//div[contains(@style, 'display: block')]//span[contains(@class, 'dialog-title')]"), 20).Text;

			Trace.WriteLine(BasePage.TraceLevelElement + "The current frame title is: '" + _title + "'");

			return _title;
		}

		public string CurrentWindowTitle()
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "The current window title is: '" + _driver.Title + "'");
			return _driver.Title;
		}

		public string CurrentWindowUrl()
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "The current window url is: '" + _driver.Url + "'");
			return _driver.Url;
		}

		public ReadOnlyCollection<IWebElement> FindElements(By locator)
		{
			Trace.WriteLine(TraceLevelLow + "Looking for elements: '" + locator + "'");
			return _driver.FindElements(locator);
		}

		public bool ElementPresent(By locator)
		{
			return _driver.ElementPresent(locator);
		}

		public void SelectSubMenuOption(string menuLocator, string subMenuLocator, string optionLocator)
		{
			Actions builder = new Actions(_driver);

			int timeOutSeconds = 30;

			DateTime startTime = DateTime.UtcNow;
			do
			{
				try
				{
					Trace.WriteLine(TraceLevelElement + "Mouse over menu option: '" + menuLocator + "'");
					builder.MoveToElement(FindElement(By.XPath(menuLocator))).Perform();

					Trace.WriteLine(TraceLevelElement + "Click on tab : '" + subMenuLocator + "'");
					FindElement(By.XPath(subMenuLocator)).WaitTillVisible(5);
					
					break;
				}
				catch (Exception) { }

				Thread.Sleep(TimeSpan.FromSeconds(1));
			}
			while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(timeOutSeconds));

			Click(By.XPath(subMenuLocator));

			Trace.WriteLine(TraceLevelElement + "Select submenu option: '" + menuLocator + "'");

			WaitForElement(By.XPath(optionLocator)).WaitTillVisible();

			Click(By.XPath(optionLocator));
		}

		public void SelectMenuOption(string menuLocator, string optionLocator)
		{
			Trace.WriteLine(TraceLevelElement + "Select menu option: '" + menuLocator + "'");

			_driver.FindElement(By.XPath(menuLocator)).Info();

			IWebElement lastElement = _driver.FindElement(By.XPath(menuLocator)).FindElement(By.XPath("./following-sibling::*/li[last()]")); 
			Point lastElementStart = lastElement.Location;

			WaitAndClick(By.XPath(menuLocator));

			BasePage.WaitTillStopMoving(_driver, lastElement, lastElementStart);

			_driver.FindElement(By.XPath(optionLocator)).Info();
			Click(By.XPath(optionLocator));

			Thread.Sleep(1000);
		}

		public void WaitForConfirmationBox(int timeOutSeconds)
		{
			Trace.WriteLine(TraceLevelElement + "Wait for Confirmation box:");
			WaitForElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]"));
		}

		public void ClickYesOnConfirmationBox()
		{
			Trace.WriteLine(TraceLevelElement + "Click 'Yes' in Confirmation box:");
			try
			{
				WaitForElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//button[@type = 'button']/span[contains(text(), 'Yes')]")).Click();
			}
			catch (Exception)
			{
				Trace.WriteLine(BasePage.TraceLevelElement + "The HTTP request to the remote WebDriver server for URL timed out");
			}
		}

		public void OpenTab(By tabName)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on Tab:");
			//WaitForElement(tabName);
			//FindElement(By.XPath(BasePage.SearchBox)).ScrollIntoView().WaitTillVisible();
			//Click(tabName);

			IWebElement tab = BasePage.WaitForElement(_driver, tabName);
			if (tab.Displayed)
			{
				FindElement(By.XPath(BasePage.SearchBox)).ScrollIntoView().WaitTillVisible();
				FindElement(tabName).Click();
			}
		}

		public void ClickOnButton(By buttonName)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on button:");
			//FindElement(By.Id(BasePage.CopyrightNotice)).ScrollIntoView().WaitTillVisible();
			//Click(buttonName);

			IWebElement button = BasePage.WaitForElement(_driver, buttonName);
			if (button.Displayed)
			{
				FindElement(By.Id(BasePage.CopyrightNotice)).ScrollIntoView().WaitTillVisible();
				FindElement(buttonName).Click();
			}
		}

		public IWebElement ScrollIntoView(IWebElement element, int offset)
		{
			Trace.WriteLine(BasePage.TraceLevelLow + "Scroll into view: " + element);
			var pt = ((ILocatable)element).LocationOnScreenOnceScrolledIntoView;

			Trace.WriteLine(BasePage.TraceLevelLow + "Scroll into view with offset: " + offset);
			((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollBy(0," + (pt.Y - offset) + ");");

			return element;
		}

		#region Page Control Helpers

		public void RadioButtonSelect(By buttonId)
		{
			new RadioButton(_driver, buttonId).Select();
		}

		public void CheckBoxCheck(By checkBoxId) 
		{
			new CheckBox(_driver, checkBoxId).Check();
		}

		public void CheckBoxUncheck(By checkBoxId)
		{
			new CheckBox(_driver, checkBoxId).UnCheck();
		}

		public void SlidingSelectByValue(By arrowId, By listDropDownId, string value)
		{
			new SlidingSelect(_driver, arrowId, listDropDownId).SelectByValue(value);
		}

		public void LoadableSelectByValue(By dropDownId, string value)
		{
			new LoadableSelect(_driver, dropDownId).SelectByValue(value);
		}

		public void AccordionOpen(By accordionId)
		{
			new Accordion(_driver, accordionId).Open();
		}

		#endregion
	}

	public static class WebElementExtensions
	{
		private static IWebElement WaitTill(this IWebElement element, int timeOutSeconds, Func<IWebElement, bool> condition, string desc)
		{
			timeOutSeconds *= Settings.Default.WaitFactor;

			Trace.Write(BasePage.TraceLevelLow + desc);
			Trace.WriteLine("Max waiting time: " + timeOutSeconds + " sec");

			var wait = new DefaultWait<IWebElement>(element);
			wait.Timeout = TimeSpan.FromSeconds(timeOutSeconds);
				
			wait.Until(condition);

			return element;
		}

		public static IWebElement WaitTillVisible(this IWebElement element, int timeOutSeconds = BasePage.FindElementTimeOut)
		{
			return WaitTill(element, timeOutSeconds, e => e.Displayed, "Wait for element is visible.");
		}

		public static IWebElement WaitTillNotVisible(this IWebElement element, int timeOutSeconds = BasePage.FindElementTimeOut)
		{
			return WaitTill(element, timeOutSeconds, e => !e.Displayed, "Wait for element is NOT visible.");
		}

		public static IWebElement WaitTillEnabled(this IWebElement element, int timeOutSeconds = BasePage.FindElementTimeOut)
		{
			return WaitTill(element, timeOutSeconds, e => e.Enabled, "Wait for current element is enabled.");
		}

		public static IWebElement WaitTillDisabled(this IWebElement element, int timeOutSeconds = BasePage.FindElementTimeOut)
		{
			return WaitTill(element, timeOutSeconds, e => !e.Enabled, "Wait for current element is disabled.");
		}

		public static IWebElement ScrollIntoView(this IWebElement element)
		{
			Trace.WriteLine(BasePage.TraceLevelLow + "Scroll into View: ");
			var pt = ((ILocatable)element).LocationOnScreenOnceScrolledIntoView;
			return element;
		}
	}

	public static class WebDriverExtensions
	{
		public static bool ElementPresent (this IWebDriver driver, By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelLow + "Looking for elements: " );
			return driver.FindElements(locator).Count > 0;
		}
	}

	public static class WebElementExtension
	{
		public static string Info(this IWebElement element)
		{
			string info = "{";
			try { info += String.Format("Type: {0}; ", element.GetType().Name); }
			catch (Exception) { };
			try { info += String.Format("Tag: {0}; ", element.TagName); }
			catch (Exception) { };
			try { info += String.Format("class: {0}; ", element.GetAttribute("class")); }
			catch (Exception) { };
			try { info += String.Format("id: {0}; ", element.GetAttribute("id")); }
			catch (Exception) { };
			try { info += String.Format("Text: {0}; ", element.Text); }
			catch (Exception) { };
			try { info += String.Format("Displayed: {0}; ", element.Displayed); }
			catch (Exception) { };
			try { info += String.Format("Location: {0}:{1}; ", element.Location.X, element.Location.Y); }
			catch (Exception) { };
			try { info += String.Format("Size: {0}x{1}; ", element.Size.Width, element.Size.Height); }
			catch (Exception) { };
			info += "}";
			return info;
		}
	}
}
