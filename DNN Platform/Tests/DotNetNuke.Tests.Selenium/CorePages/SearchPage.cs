using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class SearchPage : BasePage
	{
		public SearchPage (IWebDriver driver) : base (driver) {}

		public static string SearchPageUrl = "SearchResults/tabid/87/Default.aspx?Search=home";

		public static string PageTitleLabel = "Search Results";
		public static string PageHeader = "Search Results";

		public static string ResultsList = "//div[@class = 'dnnSearchResultContainer']/div[@class = 'dnnSearchResultItem']";
		public static string TitleOfFirstFoundElement =
			"//div[@class = 'dnnSearchResultContainer']//div[@class = 'dnnSearchResultItem-Title']";
		public static string ResultNumber = "//div[contains(@class, 'dnnSearchResultPagerTop')]/div[@class = 'dnnLeft']/span";

		public void OpenSearchPage(string baseUrl)
		{

		}

	}
}
