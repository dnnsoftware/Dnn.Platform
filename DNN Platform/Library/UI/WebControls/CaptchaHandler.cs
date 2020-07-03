// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mime;
using System.Web;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      CaptchaHandler
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CaptchaHandler control provides a validator to validate a CAPTCHA Challenge
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CaptchaHandler : IHttpHandler
    {
        private const int MAX_IMAGE_WIDTH = 600;
        private const int MAX_IMAGE_HEIGHT = 600;

        #region IHttpHandler Members

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            NameValueCollection queryString = context.Request.QueryString;
            string text = queryString[CaptchaControl.KEY];
            HttpResponse response = context.Response;
            response.ContentType = MediaTypeNames.Image.Jpeg;
            Bitmap bmp = CaptchaControl.GenerateImage(text);
            if (bmp != null)
            {
                bmp.Save(response.OutputStream, ImageFormat.Jpeg);
            }
        }

        #endregion
    }
}
