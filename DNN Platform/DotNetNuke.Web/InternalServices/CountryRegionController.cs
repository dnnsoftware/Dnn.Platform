using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
	[AllowAnonymous]
	public class CountryRegionController : DnnApiController
	{

		[HttpGet()]
		public HttpResponseMessage Countries()
		{
			var searchString = (HttpContext.Current.Request.Params["SearchString"] ?? "").NormalizeString();
            var countries = CachedCountryList.GetCountryList(Thread.CurrentThread.CurrentCulture.Name);
			return Request.CreateResponse(HttpStatusCode.OK, countries.Values.Where(
                x => x.NormalizedFullName.IndexOf(searchString, StringComparison.CurrentCulture) > -1).OrderBy(x => x.NormalizedFullName));
		}

		public struct Region
		{
			public string Text;
			public string Value;
		}

		[HttpGet()]
		public HttpResponseMessage Regions(int country)
		{
			List<Region> res = new List<Region>();
			foreach (ListEntryInfo r in (new ListController()).GetListEntryInfoItems("Region").Where(l => l.ParentID == country))
			{
				res.Add(new Region
				{
					Text = r.Text,
					Value = r.EntryID.ToString()
				});
			}
			return Request.CreateResponse(HttpStatusCode.OK, res.OrderBy(r => r.Text));
		}

	}
}
