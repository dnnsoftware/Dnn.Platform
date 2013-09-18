#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;
using System.Globalization;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.IO;
using DotNetNuke.Services.Localization.Internal;

#endregion

namespace DotNetNuke.Services.UserProfile
{
    public class UserProfilePicHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            SetupCulture();

            var userId = -1;
            var width = 55;
            var height = 55;
            var size = "";
            try
            {
                if (!String.IsNullOrEmpty(context.Request.QueryString["userid"]))
                {
                    userId = Convert.ToInt32(context.Request.QueryString["userid"]);
                }

                if (!String.IsNullOrEmpty(context.Request.QueryString["w"]))
                {
                    width = Convert.ToInt32(context.Request.QueryString["w"]);
                }

                if (!String.IsNullOrEmpty(context.Request.QueryString["h"]))
                {
                    height = Convert.ToInt32(context.Request.QueryString["h"]);
                }
                if (!String.IsNullOrEmpty(context.Request.QueryString["size"]))
                {
                    size = context.Request.QueryString["size"];
                }

            }
            catch (Exception)
            {
                Exceptions.Exceptions.ProcessHttpException(context.Request);
            }

            if (height > 64) {height = 64;}
            if (width > 64) { width = 64; }

           
            CalculateSize(ref height, ref width, ref size);

            PortalSettings settings = PortalController.GetCurrentPortalSettings();
            var userController = new UserController();
            var user = userController.GetUser(settings.PortalId, userId);
            
            FileInfo fileInfo ;
            string ext;
            if (user == null)
            {
                fileInfo = new FileInfo(context.Request.MapPath("~/images/no_avatar.gif"));
                ext = ".gif";
            }
            else
            {              
                fileInfo = new FileInfo(context.Request.MapPath(user.Profile.PhotoURLFile));
               
                if (fileInfo.Exists)
                {
                    ext = fileInfo.Extension;
                }
                else
                {
                    fileInfo = new FileInfo(context.Request.MapPath("~/images/no_avatar.gif"));
                    ext = ".gif";
                }
            }
           
            string sizedPhoto = fileInfo.FullName.Replace(ext, "_" + size + ext);

            if (IsImageExtension(ext) != true)
            {
                context.Response.End();
            }

            if (!File.Exists(sizedPhoto))
            {
                try
                {
                    //need to create the photo
                    File.Copy(fileInfo.FullName, sizedPhoto);
                    sizedPhoto = ImageUtils.CreateImage(sizedPhoto, height, width);
                }
                catch (Exception)
                {
                    //do nothing - stops exception when 2 requests on one page compete to copy large file
                }
               
            }

            switch (ext)
            {
                case ".png":
                    context.Response.ContentType = "image/png";
                    break;
                case ".jpeg":
                case ".jpg":
                    context.Response.ContentType = "image/jpeg";
                    break;
                case ".gif":
                    context.Response.ContentType = "image/gif";
                    break;

            }
            context.Response.WriteFile(sizedPhoto);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(1));
            context.Response.Cache.SetMaxAge(new TimeSpan(0, 1, 0));
            context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
            context.Response.End();

        }

        private void CalculateSize(ref int height, ref int width, ref string size)
        {
            if (height > 0 && height <= 32)
            {
                height = 32;
                width = 32;
                size = "xs";
            }
            else if (height > 32 && height <= 50)
            {
                height = 50;
                width = 50;
                size = "s";
            }
            else if (height > 50 && height <= 64)
            {
                height = 64;
                width = 64;
                size = "l";
            }
            //set a default if unprocessed
            if (String.IsNullOrEmpty(size))
            {
                height = 32;
                width = 32;
                size = "xs";
            }
        }


        private bool IsImageExtension(string extension)
        {
            List<string> imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO" };
            return imageExtensions.Contains(extension.ToUpper());
        }

        private void SetupCulture()
        {
            PortalSettings settings = PortalController.GetCurrentPortalSettings();
            if (settings == null) return;

            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(settings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, settings);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

    }
}
