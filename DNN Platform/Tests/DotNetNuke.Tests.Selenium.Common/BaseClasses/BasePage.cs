using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using DNNSelenium.Common.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.BaseClasses
{
	public class BasePage
	{
		protected readonly IWebDriver _driver;

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

		public virtual string PageHeaderLabel { get { return "Override me!!!";  } }
		public virtual string PageTitleLabel { get { return "Override me!!!"; } }
		public virtual string PreLoadedModule { get { return "Override me!!!"; } }

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

		public void SetPageToEditMode()
		{
			WaitForElement(By.XPath("//ul[@id = 'ControlEditPageMenu']"));

			if (! ElementPresent(By.XPath(ControlPanelIDs.PageInEditMode)))
			{
				Trace.WriteLine(TraceLevelPage + "Set the Page to Edit mode:");
				SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.EditThisPageButton);
				WaitForElement(By.XPath("//a[@class = 'controlBar_editPageInEditMode']"));
			}
			else
			{
				Trace.WriteLine(TraceLevelPage + "The Page is already set to Edit mode");
			}
		}

		public void CloseEditMode(string pageName)
		{
			Trace.WriteLine(TraceLevelPage + "Close Page Edit mode:");
			SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.CloseEditModeButton);
			FindElement(By.XPath(ControlPanelIDs.PageInEditMode)).WaitTillNotVisible();
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
			//Trace.WriteLine(TraceLevelLow + _driver.FindElement(locator).Info());
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

		public void WaitAndClear(By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Wait and Clear the input field");
			WaitForElement(locator).Clear();
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

			WaitForElement(By.Id(ControlPanelIDs.CopyrightNotice), timeOutSeconds);
		}

		public void ClickCloseButtonOnFrame(int timeOutSeconds)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on 'Close' button on frame and switch to window");
			_driver.SwitchTo().DefaultContent();
			FindElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//span")).WaitTillVisible().Click();
			WaitForElement(By.Id(ControlPanelIDs.CopyrightNotice), timeOutSeconds).WaitTillVisible();
		}

		public string CurrentFrameTitle()
		{
			WaitForElement(By.XPath("//div[@aria-describedby = 'iPopUp']"), 60);

			string _title = WaitForElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//span[contains(@class, '-dialog-title')]"), 30).Text;

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

			Thread.Sleep(1000);
		}

		public void OpenTab(By tabName)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on Tab:");

			IWebElement tab = BasePage.WaitForElement(_driver, tabName);
			if (!tab.Displayed)
			{
				ScrollIntoView(FindElement(tabName), 200).WaitTillVisible();
			}

			FindElement(tabName).Click();
		}

		public void ClickOnButton(By buttonName)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on button:");

			IWebElement button = BasePage.WaitForElement(_driver, buttonName);
			if (!button.Displayed)
			{
				FindElement(buttonName).ScrollIntoView().WaitTillVisible();
			}

			FindElement(buttonName).Click();
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

		public void FolderSelectByValue(By dropDownId, string value)
		{
			new FolderSelect(_driver, dropDownId).SelectUsingSearch(value);
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
			Trace.WriteLine(BasePage.TraceLevelLow + "Looking for element: " );
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
