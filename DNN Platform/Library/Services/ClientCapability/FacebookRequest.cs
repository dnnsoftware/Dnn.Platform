#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.ClientCapability
{
    /// <summary>
    /// Make modules that are aware of Facebook’s signed_request – a parameter that is POSTed to the web page being loaded in the iFrame, 
    /// giving it variables such as if the Page has been Liked, and the age range of the user.
    /// 
    /// For more details visit http://developers.facebook.com/docs/authentication/signed_request/
    /// </summary>
    public class FacebookRequest
    {
        #region Public Properties

        /// <summary>
        ///  Mechanism used to sign the request
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        ///  Token you can pass to the Graph API or the Legacy REST API.
        /// </summary>
        public string OauthToken{ get; set; }

        /// <summary>
        ///  DateTime when the oauth_token expires
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        ///  DateTime when the request was signed.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        ///  Facebook user identifier (UID) of the current user.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        ///  User's locale.
        /// </summary>
        public string UserLocale { get; set; }

        /// <summary>
        ///  User's country.
        /// </summary>
        public string UserCountry { get; set; }

        /// <summary>
        ///  User's minimum age range.
        /// </summary>
        public long UserMinAge { get; set; }

        /// <summary>
        ///  User's maximum age range.
        /// </summary>
        public long UserMaxAge { get; set; }

        /// <summary>
        /// Page's Id. Only available if your app is an iframe loaded in a Page tab.
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        ///  Has the user has liked the page. Only available if your app is an iframe loaded in a Page tab.
        /// </summary>
        public bool PageLiked { get; set; }

        /// <summary>
        ///  Is the page user Admin of the page. Only available if your app is an iframe loaded in a Page tab.
        /// </summary>
        public bool PageUserAdmin { get; set; }   

        /// <summary>
        /// Page ID if your app is loaded within. Only available if your app is written in FBML and loaded in a Page tab.
        /// </summary>
        public long ProfileId { get; set; }

        /// <summary>
        /// Content of a query string parameter also called app_data. Usually specified when the application built the link to pass some data to itself. Only available if your app is an iframe loaded in a Page tab.
        /// </summary>
        public string AppData { get; set; }  

        /// <summary>
        /// Raw signed request coming from FaceBook in Post
        /// </summary>
        public string RawSignedRequest { get; set; }

        /// <summary>
        /// Is this a valid FaceBook Request. Check this value prior to accessing any other property
        /// </summary>
        public bool IsValid { get; set; }
        #endregion

        #region Public Methods    
        public bool IsValidSignature (string secretKey)
        {
            return FacebookRequestController.IsValidSignature(RawSignedRequest,secretKey);
        }
        #endregion        
    }
    
}
