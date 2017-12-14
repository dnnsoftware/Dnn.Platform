// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetNuke.Web.Api
{
    /// <summary>
    /// Represents a non successful response while executing a WebApi call.
    /// </summary>
    public class WebApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the WebApiException class, with the specified inner exception and response
        /// message that caused the exception
        /// </summary>
        /// <param name="innerException">The original exception</param>
        /// <param name="result">The result of the request</param>
        public WebApiException(Exception innerException, HttpResponseMessage result)
            : base(innerException.Message, innerException)
        {
            Result = result;
        }

        public WebApiException(Exception innerException, HttpResponseMessage result, string body)
            : this(innerException, result)
        {
            Body = body;
        }

        /// <summary>
        /// The result of the request. Can be used to retrieve additional info like HTTP status code
        /// </summary>
        public HttpResponseMessage Result { get; private set; }

        /// <summary>
        /// Body from the Get Response. Available when exception is thrown as well.
        /// </summary>
        public string Body { get; }

        public dynamic BodyAsJson()
        {
            return JsonConvert.DeserializeObject<JContainer>(Body);
        }
    }
}
