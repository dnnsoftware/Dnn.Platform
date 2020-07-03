// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Specialized;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    public class DnnGenericHiddenField<T> : HiddenField
        where T : class, new()
    {
        private T _typedValue = null;

        private bool _isValueSerialized = false;

        public T TypedValueOrDefault
        {
            get
            {
                return this.TypedValue ?? (this.TypedValue = new T());
            }
        }

        public bool HasValue
        {
            get { return this._typedValue != null; }
        }

        public T TypedValue
        {
            get
            {
                return this._typedValue;
            }

            set
            {
                this._typedValue = value;
                this._isValueSerialized = false;
            }
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            this.EnsureValue();
            base.RenderControl(writer);
        }

        protected override object SaveViewState()
        {
            this.EnsureValue();
            return base.SaveViewState();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            this.SetTypedValue();
        }

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var controlsStateChanged = base.LoadPostData(postDataKey, postCollection);
            if (controlsStateChanged)
            {
                this.SetTypedValue();
            }

            return controlsStateChanged;
        }

        protected override void TrackViewState()
        {
            this.EnsureValue();
            base.TrackViewState();
        }

        private void SetTypedValue()
        {
            this._typedValue = string.IsNullOrEmpty(this.Value) ? null : Json.Deserialize<T>(this.Value);
        }

        private void EnsureValue()
        {
            if (!this._isValueSerialized)
            {
                this.SerializeValue();
            }
        }

        private void SerializeValue()
        {
            this.Value = this._typedValue == null ? string.Empty : Json.Serialize(this._typedValue);
            this._isValueSerialized = true;
        }
    }
}
