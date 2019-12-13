using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    public class DamExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                Exceptions.LogException(context.Exception);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, context.Exception.Message, context.Exception);
            }
        }
    }
}
