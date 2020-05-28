// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using WebFormsMvp;


#endregion

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ModuleView<TModel> : ModuleViewBase, IView<TModel> where TModel : class, new()
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

        protected override void LoadViewState(object savedState)
        {
            //Call the base class to load any View State
            base.LoadViewState(savedState);
            AttributeBasedViewStateSerializer.DeSerialize(Model, ViewState);
        }

        protected override object SaveViewState()
        {
            AttributeBasedViewStateSerializer.Serialize(Model, ViewState);
            //Call the base class to save the View State
            return base.SaveViewState();
        }
    }
}
