using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.CorePages
{
	class MessageCenterModule : BasePage
	{
		public MessageCenterModule(IWebDriver driver)
			: base(driver)
		{
		}

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public static string MessagesTab = "//a[@href = '#dnnCoreMessaging']";
		public static string NotificationsTab = "//a[@href = '#dnnCoreNotification']";

		public static string SentTab = "//ul[@class = 'dnnButtonGroup']//a/span[text() = 'Sent']";
		public static string ComposeMessageButton = "//a[contains(@class, 'dnnPrimaryAction ComposeMessage')]";
		public static string SendToTextBox = "//ul[@class = 'token-input-list-facebook']/li/input";
		public static string SearchDropDown = "//div[@class = 'token-input-dropdown-facebook' and contains(@style, 'display: block')]/ul/li";
		public static string SubjectTextBox = "//input[@id = 'subject']";
		public static string YourMessageTextBox = "//textarea[@id = 'bodytext']";
		public static string SendButton = "//button[contains(@class, 'dnnTertiaryAction')]/span[contains(text(), 'Send')]";
		public static string ReplyLink = "//li[@class = 'hoverControls']/div/a[text() = 'Reply']";
		public static string ReplyMessageTextArea = "//textarea[@id = 'replyMessage']";
		public static string ReplyButton = "//div[@class = 'dnnCoreMessagingFooter']/a";

		public static string DismissLink = "//dd[@class = 'notificationControls']/a";
		public static string NotificationSubject = "New User Registration";

		public void OpenMessagesUsingIcon()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Open 'Messages':");
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Messages' Icon:");

			WaitAndClick(By.XPath(ControlPanelIDs.MessageLink));
		}

		public void OpenNotificationsUsingIcon()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Open 'Notifications':");
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Notifications' Icon:");

			WaitAndClick(By.XPath(ControlPanelIDs.NotificationLink));
		}

		public void ComposeNewMessage(string sendTo, string subject, string message)
		{
			Trace.WriteLine(TraceLevelComposite + "Compose new message:");
			WaitAndClick(By.XPath(ComposeMessageButton));

			Trace.WriteLine(TraceLevelPage + "Find the User:");
			WaitAndType(By.XPath(SendToTextBox), sendTo);
			WaitForElement(By.XPath(SearchDropDown + "[last()]"));

			Click(By.XPath(SearchDropDown + "[contains(text(), " + sendTo + ")]"));
			
			Trace.WriteLine(TraceLevelPage + "Type in the subject:");
			Type(By.XPath(SubjectTextBox), subject);

			Trace.WriteLine(TraceLevelPage + "Type in the message:");
			Type(By.XPath(YourMessageTextBox), message);

			Trace.WriteLine(TraceLevelPage + "Click on 'Send' button:");
			FindElement(By.XPath(SendButton)).WaitTillEnabled().Click();

			WaitAndClick(By.XPath("//a[contains(text(), 'Dismiss')]"));
		}

		public void ReplyMessage(string subject, string message)
		{
			Trace.WriteLine(TraceLevelComposite + "Reply the message:");

			Trace.WriteLine(TraceLevelPage + "Make the Reply link to appear:");
			Actions builder = new Actions(_driver);
			IWebElement element = WaitForElement(By.XPath("//ul[@id = 'inbox']//li[@class = 'ListCol-3']//a"));
			builder.MoveToElement(element).Perform();

			Trace.WriteLine(TraceLevelPage + "Move mouse to displayed Reply link and Click on it:");
			WaitForElement(By.XPath(ReplyLink)).WaitTillVisible().WaitTillEnabled();
			builder.MoveToElement(
				FindElement(By.XPath(ReplyLink))).
				Click().Build().Perform();

			Trace.WriteLine(TraceLevelPage + "Type in the message:");
			Type(By.XPath(ReplyMessageTextArea), message);

			Trace.WriteLine(TraceLevelPage + "Click on 'Reply' button:");
			FindElement(By.XPath(ReplyButton)).WaitTillEnabled().Click();

			Thread.Sleep(1000);
		}

		public void DismissNotification(string subject)
		{
			WaitAndClick(By.XPath("//ul[@class = 'messages']//li[@class = 'ListCol-3'][//span[contains(text(), '" + subject + "')]]" + DismissLink));

			WaitForElement(By.XPath("//div[@class = 'dnnFormMessage dnnFormSuccess' and contains(text(), 'successfully')]"));
			Thread.Sleep(1000);
		}
	}
}
