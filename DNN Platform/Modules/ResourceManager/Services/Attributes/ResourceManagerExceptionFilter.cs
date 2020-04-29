using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Dnn.Modules.ResourceManager.Exceptions;

namespace Dnn.Modules.ResourceManager.Services.Attributes
{
    public class ResourceManagerExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception == null)
            {
                return;
            }
            var exception = actionExecutedContext.Exception;

            if (exception is FolderPermissionNotMetException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.Forbidden, 
                    new { message = exception.Message });
                return;
            }

            if (exception is NotFoundException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.NotFound,
                    new { message = exception.Message });
                return;
            }

            throw exception;
        }
    }
}