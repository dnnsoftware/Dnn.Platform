using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
	class CountryRegionController : DnnApiController
	{

		[HttpGet()]
		[DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
		public HttpResponseMessage Countries()
		{
			string searchString = HttpContext.Current.Request.Params["SearchString"].NormalizeString();
			CachedCountryList countries = CachedCountryList.GetCountryList(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
			return Request.CreateResponse(HttpStatusCode.OK, countries.Values.Where(x => x.NormalizedFullName.IndexOf(searchString) > -1).OrderBy(x => x.NormalizedFullName));
		}

		public struct Region
		{
			public string Text;
			public string Value;
		}

		[HttpGet()]
		[DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
		public HttpResponseMessage Regions(string country)
		{
			List<Region> res = new List<Region>();
			foreach (ListEntryInfo r in (new ListController()).GetListEntryInfoItems("Region", "Country." + country, ActiveModule.PortalID))
			{
				res.Add(new Region
				{
					Text = r.Text,
					Value = r.Value
				});
			}
			return Request.CreateResponse(HttpStatusCode.OK, res);
		}

		[HttpGet()]
		[DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
		public HttpResponseMessage SiblingRegions(string region)
		{
			List<Region> res = new List<Region>();
			foreach (ListEntryInfo r in GetSiblingRegions(ActiveModule.PortalID, region))
			{
				res.Add(new Region
				{
					Text = r.Text,
					Value = r.Value
				});
			}
			return Request.CreateResponse(HttpStatusCode.OK, res);
		}

		public static List<ListEntryInfo> GetSiblingRegions(int portalId, string region)
		{
			// return DotNetNuke.Common.Utilities.CBO.FillCollection<ListEntryInfo>(Albatros.DNN.Modules.Registration.Data.DataProvider.Instance().GetSiblingRegions(portalId, region));
			return new List<ListEntryInfo>();
		}


	}
}
