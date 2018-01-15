#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.IO;
using System.Threading;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization.Internal;

#endregion

namespace DotNetNuke.Services.UserProfile
{
    public class UserProfilePicHandler : IHttpHandler
    {
        #region Private Fields

        private static object _locker = new object();

        #endregion

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

            if (height > 128) { height = 128; }
            if (width > 128) { width = 128; }

           
            CalculateSize(ref height, ref width, ref size);

            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            var user = UserController.Instance.GetUser(settings.PortalId, userId);

            IFileInfo photoFile = null;
            var photoLoaded = false;
            if (user != null && TryGetPhotoFile(user, out photoFile))
            {
                if (!IsImageExtension(photoFile.Extension))
                {
	                try
	                {
						context.Response.End();
	                }
					catch (ThreadAbortException)//if ThreadAbortException will shown, should catch it and do nothing.
	                {
		                
	                }
                    
                }

                var folder = FolderManager.Instance.GetFolder(photoFile.FolderId);
                var extension = "." + photoFile.Extension;
                var sizedPhoto = photoFile.FileName.Replace(extension, "_" + size + extension);
                if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                {
                    lock (_locker)
                    {
                        if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                        {
                            using (var fileContent = FileManager.Instance.GetFileContent(photoFile))
                            using (var sizedContent = ImageUtils.CreateImage(fileContent, height, width, extension))
                            {
                                FileManager.Instance.AddFile(folder, sizedPhoto, sizedContent);
                            }
                        }
                    }
                }

                using (var content = FileManager.Instance.GetFileContent(FileManager.Instance.GetFile(folder, sizedPhoto)))
                {
                    switch (photoFile.Extension.ToLowerInvariant())
                    {
                        case "png":
                            context.Response.ContentType = "image/png";
                            break;
                        case "jpeg":
                        case "jpg":
                            context.Response.ContentType = "image/jpeg";
                            break;
                        case "gif":
                            context.Response.ContentType = "image/gif";
                            break;

                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        content.CopyTo(memoryStream);
                        memoryStream.WriteTo(context.Response.OutputStream);
                    }

                    photoLoaded = true;
                }
            }

            if (!photoLoaded)
            {
                context.Response.ContentType = "image/gif";
                context.Response.WriteFile(context.Request.MapPath("~/images/no_avatar.gif"));
            }

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(1));
            context.Response.Cache.SetMaxAge(new TimeSpan(0, 1, 0));
            context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
            context.ApplicationInstance.CompleteRequest();
        }

        //whether current user has permission to view target user's photo.
        private bool TryGetPhotoFile(UserInfo targetUser, out IFileInfo photoFile)
        {
            bool isVisible = false;
            photoFile = null;

            UserInfo user = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty != null)
            {
	            isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, targetUser);

                if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
                {
                    photoFile = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue));
                    if (photoFile == null)
                    {
                        isVisible = false;
                    }
                }
                else
                {
                    isVisible = false;
                }
            }

            return isVisible;
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
            else if (height > 64 && height <= 128)
            {
                height = 128;
                width = 128;
                size = "xl";
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
            if (!extension.StartsWith("."))
            {
                extension = string.Format(".{0}", extension);
            }

            List<string> imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO" };
            return imageExtensions.Contains(extension.ToUpper());
        }

        private void SetupCulture()
        {
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
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
