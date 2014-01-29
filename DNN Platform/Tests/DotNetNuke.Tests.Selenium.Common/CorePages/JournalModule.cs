using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.CorePages
{
	public class JournalModule : BasePage
	{
		public JournalModule(IWebDriver driver) : base(driver)
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

		public static string TextAreaPlaceholder = "//div[@id = 'journalPlaceholder']";
		public static string CommentTextAreaPlaceHolder = "//div[@class = 'editorPlaceholder']";
		public static string JournalContent = "//textarea[@id = 'journalContent']";
		public static string ShareButton = "//a[@id = 'btnShare']";
		public static string CommentLink = "//p[@class = 'journalfooter']/a[text() = 'Comment']";

		public static string AttachFileIcon = "//span[@id = 'tbar-attach']";
		public static string AttachPictureIcon = "//span[@id = 'tbar-photo']";
		public static string VisibilityPermissionIcon = "//span[@id = 'tbar-perm']";
		public static string ChooseFileButton = "//input[@id = 'uploadFileId']";
		public static string BrowseFromSiteButton = "//a[@id = 'photoFromSite']";

		public void FillInMessageText(string messageText)
		{
			Trace.WriteLine(TraceLevelPage + "Input the message text:");

			Trace.WriteLine(TraceLevelPage + "Click on Edit area:");
			WaitAndClick(By.XPath(TextAreaPlaceholder));

			Trace.WriteLine(TraceLevelPage + "Type in Edit area:");
			FindElement(By.XPath(JournalContent)).WaitTillEnabled();
			Type(By.XPath(JournalContent), messageText);
		}

		public void AddNewPost(string messageText)
		{
			Trace.WriteLine(TraceLevelComposite + "Add a new Post:");

			FillInMessageText(messageText);

			Trace.WriteLine(TraceLevelPage + "Click on Share button:");
			WaitAndClick(By.XPath(ShareButton));
		}

		public void DeletePost(string messageText)
		{
			Trace.WriteLine(TraceLevelComposite + "Delete the Post:");

			Trace.WriteLine(TraceLevelPage + "Make the Delete button to appear:");
			Actions builder = new Actions(_driver);
			IWebElement element = WaitForElement(By.XPath("//div[@class = 'journalitem']/p[text() = '" + messageText + "']"));
			builder.MoveToElement(element).Perform();

			Trace.WriteLine(TraceLevelPage + "Move mouse to displayed Delete button and Click on it:");
			WaitForElement(By.XPath("//div[@id = 'journalItems']//div[@class = 'minidel']")).WaitTillVisible().WaitTillEnabled();
			builder.MoveToElement(
				FindElement(By.XPath("//div[@id = 'journalItems']/div[div[@class = 'journalitem']/p[text() = '" + messageText + "']]/div[@class = 'minidel']"))).
				Click().Build().Perform();

			Trace.WriteLine(TraceLevelPage + "Click on 'Yes' buton to confirm delete:");
			WaitForConfirmationBox(15);
			ClickYesOnConfirmationBox();
		}

		public void CommentPost(string messageText, string commentText)
		{
			Trace.WriteLine(TraceLevelComposite + "Comment the Post:");

			Trace.WriteLine(TraceLevelPage + "Comment Link:");
			WaitAndClick(By.XPath(CommentLink));

			Trace.WriteLine(TraceLevelPage + "Click on Edit area:");
			WaitAndClick(By.XPath("//div[p[text() = '" + messageText + "']]" + CommentTextAreaPlaceHolder));

			Trace.WriteLine(TraceLevelPage + "Type in Edit area:");
			FindElement(By.XPath("//div[p[text() = '" + messageText + "']]//textarea")).WaitTillEnabled();
			Type(By.XPath("//div[p[text() = '" + messageText + "']]//textarea"), commentText);

			Trace.WriteLine(TraceLevelPage + "Click on Comment button:");
			WaitAndClick(By.XPath("//div[@id = 'journalItems']/div[div[@class = 'journalitem']/p[text() = '" + messageText + "']]" + "//li[@class = 'cmtbtn']/a"));
		}

		public void AddNewPostWithAttachedFile(string messageText, string fileToShare)
		{
			Trace.WriteLine(TraceLevelComposite + "Add a new Post with attached File:");

			FillInMessageText(messageText);

			Trace.WriteLine(TraceLevelPage + "Click on Attach Icon:");
			Click(By.XPath(AttachFileIcon));
			WaitForElement(By.XPath(ChooseFileButton)).SendKeys(Path.GetFullPath(@"P1\" + fileToShare));

			//WaitForElement(By.XPath("//div[@class = 'filePreviewArea']/img"));
			Thread.Sleep(1000);

			Trace.WriteLine(TraceLevelPage + "Click on Share button:");
			WaitAndClick(By.XPath(ShareButton));

			Thread.Sleep(1000);
		}

		public void AddNewPostWithAttachedPicture(string messageText, string pictureToShare)
		{
			Trace.WriteLine(TraceLevelComposite + "Add a new Post with attached Picture:");

			FillInMessageText(messageText);

			Trace.WriteLine(TraceLevelPage + "Click on Pictire Icon:");
			Click(By.XPath(AttachPictureIcon));
			WaitForElement(By.XPath(ChooseFileButton)).SendKeys(Path.GetFullPath(@"P1\" + pictureToShare));

			WaitForElement(By.XPath("//div[@class = 'filePreviewArea']/img[contains(@src, '" + pictureToShare + "')]"));

			Trace.WriteLine(TraceLevelPage + "Click on Share button:");
			WaitAndClick(By.XPath(ShareButton));

			Thread.Sleep(1000);
		}

		public void AddNewPostWithVisibilityPermission(string messageText, string visibilityPermission)
		{
			Trace.WriteLine(TraceLevelComposite + "Add a new Post with visibility Permission:");

			FillInMessageText(messageText);

			Click(By.XPath(VisibilityPermissionIcon));
			FindElement(By.XPath("//div[contains(@class, 'securityMenu')]")).WaitTillEnabled();

			Click(By.XPath("//li[text() = '" + visibilityPermission + "']/input"));

			Trace.WriteLine(TraceLevelPage + "Click on Share button:");
			WaitAndClick(By.XPath(ShareButton));

			Thread.Sleep(1000);
		}

		public void AddNewPostWithAttachedWebsitePicture(string messageText, string pictureToShare)
		{
			Trace.WriteLine(TraceLevelComposite + "Add a new Post with attached File:");

			FillInMessageText(messageText);

			Trace.WriteLine(TraceLevelPage + "Click on Attach Icon:");
			Click(By.XPath(AttachFileIcon));

			WaitAndClick(By.XPath(BrowseFromSiteButton));

			WaitForElement(By.XPath("//div[@id = 'dnnUserFileManager']//td/span[contains(text(), '" + pictureToShare + "')]")).WaitTillVisible().Click();
			Click(By.XPath("//ul[@class = 'dnnActions']/a[@class = 'dnnPrimaryAction']"));
			//WaitForElement(By.XPath("//div[@class = 'filePreviewArea']/img"));
			Thread.Sleep(1000);

			Trace.WriteLine(TraceLevelPage + "Click on Share button:");
			WaitAndClick(By.XPath(ShareButton));

			Thread.Sleep(1000);
		}

	}
}
