using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	public class LoginPage : BasePage
	{
		public LoginPage (IWebDriver driver) : base (driver) {}

		public static string RegisterLink = "//a[@id='dnn_dnnUser_enhancedRegisterLink']";
		public static string LoginLink = "dnn_dnnLogin_enhancedLoginLink";

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

		public static string LoginPageUrl = "/login.aspx";

		public static string PageTitleLabel = "User Log In";
		public static string PageHeader = "";


		//navigation to Login page
		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Login' page:");
			GoToUrl(baseUrl + LoginPageUrl);
		}

		public void DoLoginUsingLoginLink(string userName, string password)
		{
			//WaitForElement(By.XPath("//*[@id='" + LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Login using user credentials:");

			Trace.WriteLine(BasePage.TraceLevelElement + "Set '" + userName + "' in input field [id: " + LoginUserNameTextbox + "]");
			WaitForElement(By.Id(LoginUserNameTextbox)).SendKeys(userName);

			Trace.WriteLine(BasePage.TraceLevelElement + "Set '" + password + "' in input field [id: " + LoginPasswordTextbox + "]");
			WaitForElement(By.Id(LoginPasswordTextbox)).SendKeys(password);

			Trace.WriteLine(BasePage.TraceLevelElement + "Click on button [id: " + LoginButton + "]");
			WaitForElement(By.Id(LoginButton)).Click();
		}

		public void LoginUsingLoginLink(string userName, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using 'Login' link:");
			LetMeOut();

			WaitForElement(By.XPath("//*[@id='" + LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

			DoLoginUsingLoginLink(userName, password);
		}

		public void LoginUsingLoginLinkAndFrame(string userName, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using 'Login' link:");
			LetMeOut();

			WaitForElement(By.XPath("//*[@id='" + LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

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
		}

		public void LoginUsingUrl(string baseUrl, string username, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login using url:");
			LetMeOut();
			OpenUsingUrl(baseUrl);
			DoLoginUsingUrl(username, password);
		}

		public void LoginAsHost(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login as default host:");
			LetMeOut();
			OpenUsingUrl(baseUrl);
			DoLoginUsingUrl("host", "dnnhost");
		}

		public void LoginAsAdmin(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login as default admin:");
			LetMeOut();
			OpenUsingUrl(baseUrl);
			DoLoginUsingUrl("admin", "dnnadmin");
		}

		public void LoginAsDefaultUser(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login as default user:");
			LetMeOut();
			OpenUsingUrl(baseUrl);
			DoLoginUsingUrl("user", "dnnuser");
		}

		public void LetMeOut()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Logout");
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on button [id: " + LogoutLink + "]");
			WaitForElement(By.Id(LogoutLink), 20).Info();
			WaitForElement(By.Id(LogoutLink), 20).Click();
		}

		public void DoRegisterUsingRegisterLink(string userName, string displayName, string emailAddress, string password)
		{
			WaitForElement(By.XPath(RegisterLink), 20).WaitTillVisible(20).Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Register a User:");

			WaitAndType(By.XPath(RegisterUserNameTextBox), userName);
			Type(By.XPath(RegisterPasswordTextBox), password);
			Type(By.XPath(RegisterConfirmPasswordTextBox), password);
			Type(By.XPath(RegisterDisplayNameTextBox), displayName);
			Type(By.XPath(RegisterEmailAddressTextBox), emailAddress);

			Click(By.XPath(RegisterFrameButton));
			WaitForElement(By.XPath(RegisteredConfirmationMessage));
		}

		public void RegisterUser(string userName, string displayName, string emailAddress, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Register using 'Register' link:");
			LetMeOut();
			DoRegisterUsingRegisterLink(userName, displayName, emailAddress, password);
		}

	}
}
