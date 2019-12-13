using System;

namespace DotNetNuke.Framework
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IServicesFramework
    {
        /// <summary>
        /// Will cause anti forgery tokens to be included in the current page
        /// </summary>
        void RequestAjaxAntiForgerySupport();

        /// <summary>
        /// Will cause ajax scripts to be included in the current page
        /// </summary>
        void RequestAjaxScriptSupport();
    }
}