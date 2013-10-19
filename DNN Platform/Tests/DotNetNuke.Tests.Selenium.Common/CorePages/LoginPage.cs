using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class LoginPage : BasePage
	{
		public LoginPage (IWebDriver driver) : base (driver) {}

		public static string LoginUserNameTextbox = "dnn_ctr_Login_Login_DNN_txtUsername";
		public static string LoginPasswordTextbox = "dnn_ctr_Login_Login_DNN_txtPassword";

		public static string LoginButton = "dnn_ctr_Login_Login_DNN_cmdLogin";
		public static string RememberLoginCheckBox = "dnn_ctr_Login_Login_DNN_chkCookie";
		public static string RegisterButton = "dnn_ctr_Login_Login_DNN_registerLink";
		public static string RetrieveButton = "dnn_ctr_Login_Login_DNN_passwordLink";

		public static string RegisterUserNameTextBox = "//input[contains(@id, 'Register_userForm_Username_Username_TextBox')]";
		public static string RegisterPasswordTextBox = "//input[contains(@id, 'Register_userForm_Password_Password_TextBox')]";
		public static string RegisterConfirmPasswordTextBox = "//input[contains(@id, 'Register_userForm_PasswordConfirm_PasswordConfirm_TextBox')]";
		public static string RegisterDisplayNameTextBox = "//input[contains(@id, 'Register_userForm_DisplayName_DisplayName_TextBox')]";
		public static string RegisterEmailAddressTextBox = "//input[contains(@id, 'Register_userForm_Email_Email_TextBox')]";

		public static string RegisterFrameButton = "//a[contains(@id, 'Register_registerButton')]";
		public static string CancelFrameButton = "//a[contains(@id, 'Register_cancelButton')]";

		public static string RegisteredConfirmationMessage = "//div[contains(@id, 'Register_UP')]/div[contains(@id, 'dnnSkinMessage')]/span[contains(@id, 'lblMessage')]";
		public static string RegisteredConfirmationMessageText = "An e-mail with your details has been sent to the website administrator for verification. " +
		                                                         "Once your registration has been approved an e-mail will be sent to your e-mail address: email@mail.com. " +
		                                                         "In the meantime you can continue to browse this site by closing the popup.";
		public static string NotAuthorizedWarningMessage = "//div[contains(@class, 'dnnFormValidationSummary')]/span";
		public static string NotAuthorizedWarningMessageText = "You are not currently authorized to login to this site.";

		public static string LoginPageUrl = "/login.aspx";

		public override string PageTitleLabel
		{
			get { return "User Log In"; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return "AccountLoginModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Login' page:");
			GoToUrl(baseUrl + LoginPageUrl);
		}

		public void DoLoginUsingLoginLink(string userName, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Login using user credentials:");

			Trace.WriteLine(BasePage.TraceLevelElement + "Set '" + userName + "' in input field [id: " + LoginUserNameTextbox + "]");
			WaitForElement(By.Id(LoginUserNameTextbox)).SendKeys(userName);

			Trace.WriteLine(BasePage.TraceLevelElement + "Set '" + password + "' in input field [id: " + LoginPasswordTextbox + "]");
			WaitForElement(By.Id(LoginPasswordTextbox)).SendKeys(password);

			Trace.WriteLine(BasePage.TraceLevelElement + "Click on button [id: " + LoginButton + "]");
			WaitForElement(By.Id(LoginButton)).Click();

			Thread.Sleep(1000);
		}

		public void LoginUsingLoginLink(string userName, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using 'Login' link:");
			LetMeOut();

			FindElement(By.XPath(ControlPanelIDs.LoginLink)).Click();
			DoLoginUsingLoginLink(userName, password);
		}

		public void LoginUsingLoginLinkAndFrame(string userName, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using 'Login' link:");

			LetMeOut();

			string selector = null;

			if (ElementPresent(By.XPath(ControlPanelIDs.CompanyLogo)))
			{
				selector = WaitForElement(By.XPath(ControlPanelIDs.CompanyLogo)).GetAttribute("src");

			}

			Trace.WriteLine(BasePage.TraceLevelElement + selector);

			if (selector == null || selector.EndsWith(ControlPanelIDs.AwesomeCycles))
			{
				Trace.WriteLine(BasePage.TraceLevelElement + "Click on : " + ControlPanelIDs.LoginLink + "]");
				FindElement(By.XPath(ControlPanelIDs.LoginLink)).Click();
			}
			else
			{
				Trace.WriteLine(BasePage.TraceLevelElement + "Click on : " + ControlPanelIDs.SocialUserLink + "]");
				FindElement(By.XPath(ControlPanelIDs.SocialLoginLink)).Click();
			} 

			WaitAndSwitchToFrame(30);
			
			DoLoginUsingLoginLink(userName, password);

			WaitAndSwitchToWindow(30);
		}

		public void DoLoginUsingUrl(string username, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Login using user credentials:");

			WaitAndType(By.Id(LoginUserNameTextbox), username);
			WaitAndType(By.Id(LoginPasswordTextbox), password);

			Click(By.Id(LoginButton));

			Thread.Sleep(1000);
		}

		public void LoginUsingUrl(string baseUrl, string username, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using url:");

			//OpenUsingUrl(baseUrl);

			LetMeOut();

			LoginUsingLoginLink(username, password);
		}

		public void LoginAsHost(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login as default host:");
			GoToUrl(baseUrl);
			LoginUsingUrl(baseUrl, "host", "dnnhost");
		}

		public void LetMeOut()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Logout");

			string selector = null;

			if (ElementPresent(By.XPath(ControlPanelIDs.CompanyLogo)))
			{
				selector = WaitForElement(By.XPath(ControlPanelIDs.CompanyLogo)).GetAttribute("src");
			}
			
			Trace.WriteLine(BasePage.TraceLevelElement + selector);

			if (selector == null || selector.EndsWith(ControlPanelIDs.AwesomeCycles))
			{
				WaitForElement(By.Id(ControlPanelIDs.LogoutLinkID), 20).Info();

				Trace.WriteLine(BasePage.TraceLevelElement + "Click on button: " + ControlPanelIDs.LogoutLink + "]");
				if (ElementPresent(By.XPath(ControlPanelIDs.LogoutLink)))
				{
					FindElement(By.XPath(ControlPanelIDs.LogoutLink)).Click();
					WaitForElement(By.XPath(ControlPanelIDs.LoginLink), 20).WaitTillVisible(20);
				}
			}
			else
			{
				if (ElementPresent(By.XPath(ControlPanelIDs.SocialUserLink)))
				{
					Trace.WriteLine(BasePage.TraceLevelElement + "Click on : " + ControlPanelIDs.SocialUserLink + "]");
					WaitAndClick(By.XPath(ControlPanelIDs.SocialUserLink));
					Trace.WriteLine(BasePage.TraceLevelElement + "Click on : " + ControlPanelIDs.SocialLogoutLink + "]");
					WaitAndClick(By.XPath(ControlPanelIDs.SocialLogoutLink));
					WaitForElement(By.XPath(ControlPanelIDs.SocialLoginLink));
				}		
			}
		}

		public void DoRegisterUsingRegisterLink(string userName, string displayName, string emailAddress, string password)
		{
			string selector = null;

			if (ElementPresent(By.XPath(ControlPanelIDs.CompanyLogo)))
			{
				selector = WaitForElement(By.XPath(ControlPanelIDs.CompanyLogo)).GetAttribute("src");
				
			}
			
			Trace.WriteLine(BasePage.TraceLevelElement + selector);

			if (selector == null || selector.EndsWith(ControlPanelIDs.AwesomeCycles))
			{
				WaitForElement(By.XPath(ControlPanelIDs.RegisterLink), 20).WaitTillVisible(20).Click();
			}
			else
			{
				WaitForElement(By.XPath(ControlPanelIDs.SocialRegisterLink), 20).WaitTillVisible(20).Click();
			}

			/*WaitForElement(By.XPath(ControlPanelIDs.RegisterLink), 20).WaitTillVisible(20).Click();*/

			Trace.WriteLine(BasePage.TraceLevelPage + "Register a User:");

			WaitAndType(By.XPath(RegisterUserNameTextBox), userName);
			Type(By.XPath(RegisterPasswordTextBox), password);
			Type(By.XPath(RegisterConfirmPasswordTextBox), password);
			Type(By.XPath(RegisterDisplayNameTextBox), displayName);
			Type(By.XPath(RegisterEmailAddressTextBox), emailAddress);

			Click(By.XPath(RegisterFrameButton));
			//WaitForElement(By.XPath(RegisteredConfirmationMessage));
			Thread.Sleep(1000);

		}

		public void RegisterUser(string userName, string displayName, string emailAddress, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Register using 'Register' link:");
			LetMeOut();
			DoRegisterUsingRegisterLink(userName, displayName, emailAddress, password);
		}

	}
}
