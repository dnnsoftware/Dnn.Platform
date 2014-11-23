#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Data;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Personalization
{
    public class PersonalizationController
    {
        //default implementation relies on HTTPContext
        public void LoadProfile(HttpContext httpContext, int userId, int portalId)
        {
            LoadProfile(new HttpContextWrapper(httpContext), userId, portalId);
        }

        public void LoadProfile(HttpContextBase httpContext, int userId, int portalId)
        {
            if (HttpContext.Current.Items["Personalization"] == null)
            {
                httpContext.Items.Add("Personalization", LoadProfile(userId, portalId));
            }
        }

        //override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public PersonalizationInfo LoadProfile(int userId, int portalId)
        {
            var personalization = new PersonalizationInfo {UserId = userId, PortalId = portalId, IsModified = false};
            string profileData = Null.NullString;
            if (userId > Null.NullInteger)
            {
                IDataReader dr = null;
                try
                {
                    dr = DataProvider.Instance().GetProfile(userId, portalId);
                    if (dr.Read())
                    {
                        profileData = dr["ProfileData"].ToString();
                    }
                    else //does not exist
                    {
                        DataProvider.Instance().AddProfile(userId, portalId);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
            }
            else
            {
				//Anon User - so try and use cookie.
                HttpContext context = HttpContext.Current;
                if (context != null && context.Request.Cookies["DNNPersonalization"] != null)
                {
                    profileData = context.Request.Cookies["DNNPersonalization"].Value;
                }
            }
            if (string.IsNullOrEmpty(profileData))
            {
                personalization.Profile = new Hashtable();
            }
            else
            {
                personalization.Profile = Globals.DeserializeHashTableXml(profileData);
            }
            return personalization;
        }

        public void SaveProfile(PersonalizationInfo personalization)
        {
            SaveProfile(personalization, personalization.UserId, personalization.PortalId);
        }

        //default implementation relies on HTTPContext
        public void SaveProfile(HttpContext httpContext, int userId, int portalId)
        {
            var objPersonalization = (PersonalizationInfo) httpContext.Items["Personalization"];
            SaveProfile(objPersonalization, userId, portalId);
        }

        //override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public void SaveProfile(PersonalizationInfo personalization, int userId, int portalId)
        {
            if (personalization != null)
            {
                if (personalization.IsModified)
                {
                    string ProfileData = Globals.SerializeHashTableXml(personalization.Profile);
                    if (userId > Null.NullInteger)
                    {
                        DataProvider.Instance().UpdateProfile(userId, portalId, ProfileData);
                    }
                    else
                    {
						//Anon User - so try and use cookie.
                        HttpContext context = HttpContext.Current;
                        if (context != null)
                        {
                            var personalizationCookie = new HttpCookie("DNNPersonalization")
                                                            {Value = ProfileData, Expires = DateTime.Now.AddDays(30)};
                            context.Response.Cookies.Add(personalizationCookie);
                        }
                    }
                }
            }
        }
    }
}