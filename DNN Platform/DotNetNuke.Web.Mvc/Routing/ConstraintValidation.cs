using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc.Routing
{
    /// <summary>
    /// Validates that the constraints on a Route are of a type that can be processed by <see cref="System.Web.Routing.Route" />.
    /// </summary>
    /// <remarks>
    /// This validation is only applicable when the <see cref="System.Web.Routing.Route" /> is one that we created. A user-defined
    /// type that is derived from <see cref="System.Web.Routing.RouteBase" /> may have different semantics.
    /// 
    /// The logic here is duplicated from System.Web, but we need it to validate correctness of routes on startup. Since we can't 
    /// change System.Web, this just lives in a static class for MVC.
    /// </remarks>
    internal static class ConstraintValidation
    {
        public static void Validate(Route route)
        {
            Contract.Assert(route != null);
            Contract.Assert(route.Url != null);

            if (route.Constraints == null)
            {
                return;
            }

            foreach (var kvp in route.Constraints.Where(kvp => !(kvp.Value is string)).Where(kvp => !(kvp.Value is IRouteConstraint)))
            {
                throw new InvalidOperationException("Invalid Constraint", new Exception(typeof(IRouteConstraint).FullName));
            }
        }
    }
}
