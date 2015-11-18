using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Web.Api
{
    public class HmacAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private const string AuthenticationScheme = "AMX";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HmacAuthenticationAttribute));
        private static readonly MD5 Md5 = MD5.Create();
        private static readonly DateTime EpochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
        private readonly DataProvider _dataProvider = DataProvider.Instance();
        private readonly UserController _usrController = new UserController();

        public bool AllowMultiple => false;

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            var req = context.Request;
            var authHdr = req.Headers.Authorization;
            if (authHdr != null && AuthenticationScheme.Equals(authHdr.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var autherizationHeaderArray = GetAutherizationHeaderValues(authHdr.Parameter ?? "");
                if (autherizationHeaderArray != null)
                {
                    var isValid = IsValidRequest(req, autherizationHeaderArray);
                    if (isValid.Result)
                    {
                        var validatedUser = _usrController.GetUserByHmacAppId(autherizationHeaderArray[0]);
                        if (validatedUser != null)
                        {
                            var currentPrincipal = new GenericPrincipal(new GenericIdentity(validatedUser.Username), null);
                            context.Principal = currentPrincipal;
                            context.ErrorResult = null;
                        }
                        else if (Logger.IsTraceEnabled)
                        {
                            Logger.Trace("Couldn't find HMAC user");
                        }
                    }
                    //else if (Logger.IsTraceEnabled) -- issues logged in the IsValidRequest call
                }
                else if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invalid authentication header value (must have 4 separate parts joined by ':')");
                }
            }
            else if (Logger.IsTraceEnabled)
            {
                Logger.Trace(authHdr == null
                    ? "Missing authentication header"
                    : "Invalid authentication header scheme");
            }
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultWithChallenge(context.Result);
            return Task.FromResult(0);
        }

        private static string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            var credArray = rawAuthzHeader.Split(':');
            return credArray.Length == 4 ? credArray : null;
        }

        private async Task<bool> IsValidRequest(HttpRequestMessage req, string[] autherizationHeaderArray)
        {
            var appId = autherizationHeaderArray[0];
            var incomingBase64Signature = autherizationHeaderArray[1];
            var nonce = autherizationHeaderArray[2];
            var requestTimeStamp = autherizationHeaderArray[3];

            if (IsReplayRequest(appId, nonce))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Replay request");
                }
                return false;
            }

            if (IsStaleRequest(requestTimeStamp))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Stale request (came too late). If this is repeated, check the server time.");
                }
                return false;
            }

            var requestContentBase64String = "";
            var hash = await ComputeMd5Hash(req.Content);
            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            var requestUri = HttpUtility.UrlEncode(req.RequestUri.AbsoluteUri.ToLower());
            var requestHttpMethod = req.Method.Method.ToUpper();
            var sharedSecret = _dataProvider.GetHmacSecretByHmacAppId(appId);
            var data = string.Concat(appId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);
            var signature = Encoding.UTF8.GetBytes(data);
            var sharedSecretBytes = Convert.FromBase64String(sharedSecret);
            using (var hmac = new HMACSHA256(sharedSecretBytes))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var comoutedSignature = Convert.ToBase64String(signatureBytes);
                var areSigsEqual = incomingBase64Signature.Equals(comoutedSignature, StringComparison.Ordinal);
                if (!areSigsEqual && Logger.IsTraceEnabled)
                {
                    Logger.TraceFormat("Received signature [{0}] and computed signature [{1}] are different\nRequest: {2}",
                        incomingBase64Signature, comoutedSignature, req);
                }
                return areSigsEqual;
            }
        }

        private static bool IsReplayRequest(string appId, string nonce)
        {
            var cacheKey = string.Format(DataCache.HmacCacheKey, appId + nonce);
            var cacheObj = DataCache.GetCache(cacheKey);
            if (cacheObj != null)
            {
                return true;
            }

            DataCache.SetCache(cacheKey, nonce, TimeSpan.FromSeconds(DataCache.HmacCacheTimeout));
            return false;
        }

        private static bool IsStaleRequest(string requestTimeStamp)
        {
            long requestTotalSeconds;
            // if parsing fails, it will be 0 and fails the time difference check
            long.TryParse(requestTimeStamp, out requestTotalSeconds);
            var timeDifference = (long)(DateTime.UtcNow - EpochStart).TotalSeconds;
            return Math.Abs(timeDifference - requestTotalSeconds) > DataCache.HmacCacheTimeout;
        }

        private static async Task<byte[]> ComputeMd5Hash(HttpContent httpContent)
        {
            var content = await httpContent.ReadAsByteArrayAsync();
            return content.Length == 0 ? null : Md5.ComputeHash(content);
        }

        private class ResultWithChallenge : IHttpActionResult
        {
            private readonly IHttpActionResult _next;

            public ResultWithChallenge(IHttpActionResult next)
            {
                _next = next;
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = await _next.ExecuteAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthenticationScheme));
                }

                return response;
            }
        }
    }
}
