using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Api.Internal
{
    internal class DnnActionFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            //Requires.NotNull("configuration", configuration);
            Requires.NotNull("actionDescriptor", actionDescriptor);

            var controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Controller));
            var actionFilters = actionDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Action));

            var allFilters = controllerFilters.Concat(actionFilters).ToList();

            var overrideFilterPresent = allFilters.Any(x => x.Instance is IOverrideDefaultAuthLevel);

            if(!overrideFilterPresent)
            {
                allFilters.Add(new FilterInfo(new RequireHostAttribute(), FilterScope.Action));
            }

            return allFilters;
        }
    }
}