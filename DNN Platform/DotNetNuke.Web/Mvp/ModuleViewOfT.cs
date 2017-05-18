#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using WebFormsMvp;


#endregion

namespace DotNetNuke.Web.Mvp
{
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