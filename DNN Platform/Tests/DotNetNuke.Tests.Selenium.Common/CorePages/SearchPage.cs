using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class SearchPage : BasePage
	{
		public SearchPage (IWebDriver driver) : base (driver) {}

		public static string SearchPageUrl = "SearchResults/tabid/87/Default.aspx?Search=home";

		public override string PageTitleLabel
		{
			get { return "Search Results"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Search Results"; }
		}

		public override string PreLoadedModule
		{
			get { return "SearchResultsModule"; }
		}

		public static string ResultsList = "//div[@class = 'dnnSearchResultContainer']/div[@class = 'dnnSearchResultItem']";
		public static string TitleOfFirstFoundElement =
			"//div[@class = 'dnnSearchResultContainer']//div[@class = 'dnnSearchResultItem-Title']";
		public static string ResultNumber = "//div[contains(@class, 'dnnSearchResultPagerTop')]/div[@class = 'dnnLeft']/span";

		public void OpenSearchPage(string baseUrl)
		{

		}

	}
}
