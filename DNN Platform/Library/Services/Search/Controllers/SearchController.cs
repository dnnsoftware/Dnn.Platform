#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
	/// <summary>
	/// Business Layer to manage Search.
	/// </summary>
    public class SearchController : ServiceLocator<ISearchController, SearchController>
    {
        protected override Func<ISearchController> GetFactory()
        {
            return () => new SearchControllerImpl();
        }
    }
}
