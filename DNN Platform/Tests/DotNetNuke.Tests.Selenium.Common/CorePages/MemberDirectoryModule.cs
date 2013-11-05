using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class MemberDirectoryModule : BasePage
	{
		public MemberDirectoryModule(IWebDriver driver) : base(driver) { }

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public static string AddAsFriendLink = "//";
		public static string FriendsNotFoundMessage = "//div[@class = 'dnnFormMessage dnnFormInfo']";
		public static string FriendsNotFoundMessageText = "No members were found that satisfy the search conditions.";

		public void AddAsFriend(string newFriendName)
		{
			Trace.WriteLine(TraceLevelPage + "Click on 'Add a Friend link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + newFriendName + "')]]/ul/li[@class = 'mdFriendRequest']/a"));
		}

		public void AcceptFriendRequest(string friendToAccept)
		{
			Trace.WriteLine(TraceLevelPage + "Click on 'Accept Request link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + friendToAccept + "')]]/ul/li[@class = 'mdFriendAccept']/a"));
		}

		public void RemoveFriend(string friendToRemove)
		{
			Trace.WriteLine(TraceLevelPage + "Click on 'Remove Friend link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + friendToRemove + "')]]/ul/li[@class = 'mdFriendRemove']/a"));
		}
	}
}
