// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ClientCapability
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common.Utilities;

    internal struct Page
    {
        public string id { get; set; }

        public bool liked { get; set; }

        public bool admin { get; set; }
    }

    internal struct Age
    {
        public long min { get; set; }

        public long max { get; set; }
    }

    internal struct User
    {
        public string locale { get; set; }

        public string country { get; set; }

        public Age age { get; set; }
    }

    /// <summary>
    /// Make modules that are aware of Facebook’s signed_request – a parameter that is POSTed to the web page being loaded in the iFrame,
    /// giving it variables such as if the Page has been Liked, and the age range of the user.
    ///
    /// </summary>
    public class FacebookRequestController
    {
        private const string SignedRequestParameter = "signed_request";

        public static string API_SECRET { get; set; }

        public static string APP_ID { get; set; }

        public string AccessToken { get; set; }

        public DateTime Expires { get; set; }

        public long UserID { get; set; }

        public long ProfileId { get; set; }

        public string RawSignedRequest { get; set; }

        public bool IsValid { get; set; }

        public static FacebookRequest GetFacebookDetailsFromRequest(HttpRequest Request)
        {
            if (Request == null)
            {
                return null;
            }

            if (Request.RequestType != "POST")
            {
                return null;
            }

            string rawSignedRequest = Request[SignedRequestParameter];
            return GetFacebookDetailsFromRequest(rawSignedRequest);
        }

        public static FacebookRequest GetFacebookDetailsFromRequest(string rawSignedRequest)
        {
            if (string.IsNullOrEmpty(rawSignedRequest))
            {
                return null;
            }

            try
            {
                var facebookRequest = new FacebookRequest();
                facebookRequest.RawSignedRequest = rawSignedRequest;
                facebookRequest.IsValid = false;

                string[] signedRequestSplit = rawSignedRequest.Split('.');
                string expectedSignature = signedRequestSplit[0];
                string payload = signedRequestSplit[1];

                var decodedJson = ReplaceSpecialCharactersInSignedRequest(payload);
                var base64JsonArray = Convert.FromBase64String(decodedJson.PadRight(decodedJson.Length + ((4 - (decodedJson.Length % 4)) % 4), '='));

                var encoding = new UTF8Encoding();
                FaceBookData faceBookData = encoding.GetString(base64JsonArray).FromJson<FaceBookData>();

                if (faceBookData.algorithm == "HMAC-SHA256")
                {
                    facebookRequest.IsValid = true;
                    facebookRequest.Algorithm = faceBookData.algorithm;
                    facebookRequest.ProfileId = faceBookData.profile_id;
                    facebookRequest.AppData = faceBookData.app_data;
                    facebookRequest.OauthToken = !string.IsNullOrEmpty(faceBookData.oauth_token) ? faceBookData.oauth_token : string.Empty;
                    facebookRequest.Expires = ConvertToTimestamp(faceBookData.expires);
                    facebookRequest.IssuedAt = ConvertToTimestamp(faceBookData.issued_at);
                    facebookRequest.UserID = !string.IsNullOrEmpty(faceBookData.user_id) ? faceBookData.user_id : string.Empty;

                    facebookRequest.PageId = faceBookData.page.id;
                    facebookRequest.PageLiked = faceBookData.page.liked;
                    facebookRequest.PageUserAdmin = faceBookData.page.admin;

                    facebookRequest.UserLocale = faceBookData.user.locale;
                    facebookRequest.UserCountry = faceBookData.user.country;
                    facebookRequest.UserMinAge = faceBookData.user.age.min;
                    facebookRequest.UserMaxAge = faceBookData.user.age.max;
                }

                return facebookRequest;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsValidSignature(string rawSignedRequest, string secretKey)
        {
            if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(rawSignedRequest))
            {
                string[] signedRequestSplit = rawSignedRequest.Split('.');
                string expectedSignature = signedRequestSplit[0];
                string payload = signedRequestSplit[1];

                if (!string.IsNullOrEmpty(expectedSignature) && !string.IsNullOrEmpty(payload))
                {
                    // Attempt to get same hash
                    var encoding = new UTF8Encoding();
                    var hmac = SignWithHmac(encoding.GetBytes(payload), encoding.GetBytes(secretKey));
                    var hmacBase64 = Base64UrlDecode(Convert.ToBase64String(hmac));
                    if (hmacBase64 == expectedSignature)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the base 64 url encoded string to standard base 64 encoding.
        /// </summary>
        /// <param name="encodedValue">The encoded value.</param>
        /// <returns>The base 64 string.</returns>
        private static string Base64UrlDecode(string encodedValue)
        {
            if (string.IsNullOrEmpty(encodedValue))
            {
                return null;
            }

            encodedValue = encodedValue.Replace('+', '-').Replace('/', '_').Replace("=", string.Empty).Trim();
            return encodedValue;
        }

        private static string ReplaceSpecialCharactersInSignedRequest(string str)
        {
            return str.Replace("=", string.Empty).Replace('-', '+').Replace('_', '/');
        }

        private static byte[] SignWithHmac(byte[] dataToSign, byte[] keyBody)
        {
            using (var hmacAlgorithm = new HMACSHA256(keyBody))
            {
                hmacAlgorithm.ComputeHash(dataToSign);
                return hmacAlgorithm.Hash;
            }
        }

        /// <summary>
        /// method for converting a System.DateTime value to a UNIX Timestamp.
        /// </summary>
        /// <param name="value">date to convert.</param>
        /// <returns></returns>
        private static DateTime ConvertToTimestamp(long value)
        {
            // create Timespan by subtracting the value provided from
            // the Unix Epoch
            DateTime epoc = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return epoc.AddSeconds((double)value);
        }
    }

    internal struct FaceBookData
    {
        public User user { get; set; }

        public string algorithm { get; set; }

        public long issued_at { get; set; }

        public string user_id { get; set; }

        public string oauth_token { get; set; }

        public long expires { get; set; }

        public string app_data { get; set; }

        public Page page { get; set; }

        public long profile_id { get; set; }
    }
}
