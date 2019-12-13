using System;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ModulePresenter<TView> : ModulePresenterBase<TView> where TView : class, IModuleView
    {
        #region Constructors

        protected ModulePresenter(TView view) : base(view)
        {
        }

        #endregion
    }
}
