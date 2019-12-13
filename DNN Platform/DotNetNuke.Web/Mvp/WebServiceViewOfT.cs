using System;
using WebFormsMvp;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class WebServiceViewOfT<TModel> : ModuleViewBase, IView<TModel> where TModel : class, new()
    {
        private TModel _model;

        #region IView<TModel> Members

        public TModel Model
        {
            get
            {
                if ((_model == null))
                {
                    throw new InvalidOperationException(
                        "The Model property is currently null, however it should have been automatically initialized by the presenter. This most likely indicates that no presenter was bound to the control. Check your presenter bindings.");
                }
                return _model;
            }
            set { _model = value; }
        }

        #endregion
    }
}
