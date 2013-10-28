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

		public void AddAsFriend(string newFriendName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Add a Friend link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + newFriendName + "')]]/ul/li[@class = 'mdFriendRequest']/a"));
		}

		public void AcceptFriendRequest(string friendToAccept)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Accept Request link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + friendToAccept + "')]]/ul/li[@class = 'mdFriendAccept']/a"));
		}

		public void RemoveFriend(string friendToRemove)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Remove Friend link':");
			WaitAndClick(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + friendToRemove + "')]]/ul/li[@class = 'mdFriendRemove']/a"));
		}
	}
}
