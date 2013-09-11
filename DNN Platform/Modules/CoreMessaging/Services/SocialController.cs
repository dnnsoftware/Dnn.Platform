// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    [DnnAuthorize]
	public class SocialController : DnnApiController
    {
		[HttpGet]
		[AllowAnonymous]
		public HttpResponseMessage GetLocalizationTable(string culture)
		{
			try
			{
				if (!string.IsNullOrEmpty(culture))
				{
					Localization.SetThreadCultures(new CultureInfo(culture), PortalSettings.Current);
				}

				var dictionary = new Dictionary<string, string>();

				return Request.CreateResponse(HttpStatusCode.OK, new { Table = dictionary });
			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);

				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
		}
    }
}