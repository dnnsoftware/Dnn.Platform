#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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

using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
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
            Bitmap bmp = CaptchaControl.GenerateImage(text);
            if (bmp != null)
            {
                bmp.Save(response.OutputStream, ImageFormat.Jpeg);
            }
        }

        #endregion
    }
}
