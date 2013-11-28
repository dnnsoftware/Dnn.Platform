﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.BaseClasses
{
	public class SlidingSelect
	{
		private readonly By _arrowId, _listDropDownId;
		private readonly IWebDriver _driver;

		private SlidingSelect(){}

		public SlidingSelect(IWebDriver driver, By arrowId, By listDropDownId)
		{
			_driver = driver;

			_arrowId = arrowId;
			_listDropDownId = listDropDownId;
		}

		public enum SelectByValueType
		{
			Equal,
			Contains
		};

		public void SelectByValue(string value, SelectByValueType strict = SelectByValueType.Equal)
		{
			IWebElement listDropDown = _driver.FindElement(_listDropDownId);

			IWebElement firstElement = _driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[1]"));
			IWebElement lastElement = _driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[last()]"));

			Point fistElementStart = firstElement.Location;
			Point lastElementStart = lastElement.Location;
			//_driver.FindElement(_arrowId).FindElement(By.XPath("./parent::*")).Click();
			_driver.FindElement(_arrowId).Click();

			BasePage.WaitTillStopMoving(_driver, firstElement, fistElementStart);
			BasePage.WaitTillStopMoving(_driver, lastElement, lastElementStart);

			fistElementStart = firstElement.Location;
			lastElementStart = lastElement.Location;
			if (strict == SelectByValueType.Equal)
			{
				_driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[text() = '" + value + "']")).
								ScrollIntoView().
								Click();
			}
			else if (strict == SelectByValueType.Contains)
			{
				_driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[contains(text(), '" + value + "')]")).ScrollIntoView().
					Click();
			}
			else
			{
				throw new Exception("Wrong SelectByValueType");
			}

			try
			{
				BasePage.WaitTillStopMoving(_driver, firstElement, fistElementStart);
				BasePage.WaitTillStopMoving(_driver, lastElement, lastElementStart);
			}
			catch (StaleElementReferenceException)
			{
				Trace.WriteLine(BasePage.TraceLevelElement + "StaleElementReferenceException in SelectByValue of '" + value + "'");
			}

		}
			
		public void SelectCheckBoxByValue(string value)
		{
			IWebElement listDropDown = _driver.FindElement(_listDropDownId);

			IWebElement firstElement = _driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[1]"));
			IWebElement lastElement = _driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li[last()]"));

			Point fistElementStart = firstElement.Location;
			Point lastElementStart = lastElement.Location;
			
			_driver.FindElement(_arrowId).Click();

			BasePage.WaitTillStopMoving(_driver, firstElement, fistElementStart);
			BasePage.WaitTillStopMoving(_driver, lastElement, lastElementStart);

			fistElementStart = firstElement.Location;
			lastElementStart = lastElement.Location;

			_driver.FindElement(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li/label[text() = '" + value + "']")).
								ScrollIntoView().
								Click();

			Thread.Sleep(1000);

			try
			{
				BasePage.WaitTillStopMoving(_driver, firstElement, fistElementStart);
				BasePage.WaitTillStopMoving(_driver, lastElement, lastElementStart);
			}
			catch (StaleElementReferenceException)
			{
				Trace.WriteLine(BasePage.TraceLevelElement + "StaleElementReferenceException in SelectByValue of '" + value + "'");
			}

		}

		public int CountAllOptions ()
		{
			IWebElement listDropDown = _driver.FindElement(_listDropDownId);
			int xpathCount = _driver.FindElements(By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']//li")).Count();
			return xpathCount;
		}

		public static void SelectByValue (IWebDriver driver, By arrowId, By listDropDownId, string value, SelectByValueType strict = SelectByValueType.Equal)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select '" + value + "' in the list '" + listDropDownId + "'");
			new SlidingSelect(driver, arrowId, listDropDownId).SelectByValue(value, strict);
		}

		public static void SelectCheckBoxByValue(IWebDriver driver, By arrowId, By listDropDownId, string value)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select '" + value + "' in the list '" + listDropDownId + "'");
			new SlidingSelect(driver, arrowId, listDropDownId).SelectCheckBoxByValue(value);
		}

		public static int CountAllOptions (IWebDriver driver, By arrowId, By listDropDownId)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Count options in the list '" + listDropDownId + "'");
			return new SlidingSelect(driver, arrowId, listDropDownId).CountAllOptions();
		}
	}

	public class LoadableSelect
	{
		private readonly By _dropDownId;
		private readonly IWebDriver _driver;

		private LoadableSelect(){}

		public LoadableSelect(IWebDriver driver, By dropDownId)
		{
			_driver = driver;
			_dropDownId = dropDownId;
		}

		public void SelectByValue(string value)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select '" + value + "' in the list '" + _dropDownId + "'");

			BasePage.WaitForElement(_driver,_dropDownId).Click();
			BasePage.WaitForElement(_driver, By.XPath("//div[contains(@class, '-tree')]//a[text() = '" + value + "']")).ScrollIntoView().Click();
			BasePage.WaitForElement(_driver, By.XPath("//a[@class = 'selected-value' and text() ='" + value + "']")).Info();
		}

		public static void SelectByValue (IWebDriver driver, By dropDownId, string value)
		{
			new LoadableSelect(driver, dropDownId).SelectByValue(value);
		}
	}

	public class FolderSelect
	{
		private readonly By _dropDownId;
		private readonly IWebDriver _driver;

		private FolderSelect()
		{
		}

		public FolderSelect(IWebDriver driver, By dropDownId)
		{
			_driver = driver;
			_dropDownId = dropDownId;
		}

		public void SelectUsingSearch(string value)
		{
			IWebElement listDropDown = _driver.FindElement(_dropDownId);

			Trace.WriteLine(BasePage.TraceLevelElement + "Search '" + value + "' in the list '" + _dropDownId + "'");

			BasePage.WaitForElement(_driver, By.XPath("//div[@id='" + listDropDown.GetAttribute("id") + "']" + "/div/a")).Click();

			BasePage.WaitForElement(_driver, By.XPath("//*[@id='" + listDropDown.GetAttribute("id") + "']" + "//input[@class = 'search-input']"));
			_driver.FindElement(By.XPath("//div[@id='" + listDropDown.GetAttribute("id") + "']" + "//input[@class = 'search-input']")).SendKeys(value);
			_driver.FindElement(By.XPath("//div[@id='" + listDropDown.GetAttribute("id") + "']" + "//a[@class = 'search-button']")).Click();

			Thread.Sleep(1000);
			BasePage.WaitForElement(_driver, By.XPath("//div[contains(@class, '-tree')]//a[text() = '" + value + "']")).Click();
			BasePage.WaitForElement(_driver, By.XPath("//a[@class = 'selected-value' and text() ='" + value + "']")).Info();
		}
	}

	public class RadioButton
	{
		private readonly By _buttonId;
		private readonly IWebDriver _driver;

		private RadioButton() {}

		public RadioButton (IWebDriver driver, By buttonId)
		{
			_driver = driver;

			_buttonId = buttonId;
		}	

		public void Select ()
		{
			_driver.FindElement(_buttonId).FindElement(By.XPath("./following-sibling::*")).Click();
		}

		public static void Select(IWebDriver driver, By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select radio button '" + locator + "'");
			new RadioButton(driver, locator).Select();
		}
	}
	
	public class CheckBox
	{
		private readonly By _checkBoxId;
		private readonly IWebDriver _driver;

		private CheckBox() {}

		public CheckBox(IWebDriver driver, By checkBoxId)
		{
			_driver = driver;
			_checkBoxId = checkBoxId;
		}

		public enum ActionType
		{
			Check,
			Uncheck
		};

		public void Check()
		{
			if (!_driver.FindElement(_checkBoxId).FindElement(By.XPath("./following-sibling::*"))
				.GetAttribute("class").ToLower().Contains("checked"))
			{
				_driver.FindElement(_checkBoxId).FindElement(By.XPath("./following-sibling::*")).Click();
			}
		}

		public void UnCheck()
		{
			if (_driver.FindElement(_checkBoxId).FindElement(By.XPath("./following-sibling::*"))
				.GetAttribute("class").ToLower().Contains("checked"))
			{
				_driver.FindElement(_checkBoxId).FindElement(By.XPath("./following-sibling::*")).Click();
			}
		}

		public static void Check (IWebDriver driver, By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select check box '" + locator + "'");
			new CheckBox(driver, locator).Check();
		}

		public static void UnCheck(IWebDriver driver, By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select check box '" + locator + "'");
			new CheckBox(driver, locator).UnCheck();
		}
	}

	public class Accordion
	{
		private readonly By _accordionId;
		private readonly IWebDriver _driver;

		private Accordion()
		{
		}

		public Accordion(IWebDriver driver, By accordionId)
		{
			_driver = driver;
			_accordionId = accordionId;
		}

		public void Open()
		{
			IWebElement accordion = BasePage.WaitForElement(_driver, _accordionId);

			if (accordion.Displayed)
			{
				accordion.ScrollIntoView().WaitTillVisible();
			}

			accordion.Info();

			if ( ! accordion.GetAttribute("class").Contains("dnnSectionExpanded"))
			{
				accordion.Click();
				//BasePage.WaitForElement(_driver, accordion.GetAttribute();
				Thread.Sleep(1000);
			}
		}

		public void Close()
		{
		}
	}

}