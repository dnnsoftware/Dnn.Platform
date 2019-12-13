#region Usings

using System;

#endregion

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ViewStateAttribute : Attribute
    {
        public string ViewStateKey { get; set; }
    }
}
