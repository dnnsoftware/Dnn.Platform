using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;


namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGenericHiddenField<T> : HiddenField where T : class, new()
    {

        private T _typedValue = null;

        private bool _isValueSerialized = false;
        public T TypedValue
        {
            get
            {
                return _typedValue;
            }
            set
            {
                _typedValue = value;
                _isValueSerialized = false;
            }
        }

        public T TypedValueOrDefault
        {
            get
            {
                return TypedValue ?? (TypedValue = new T());
            }
        }

        public bool HasValue
        {
            get { return _typedValue != null; }
        }

        protected override object SaveViewState()
        {
            EnsureValue();
            return base.SaveViewState();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            SetTypedValue();
        }

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var controlsStateChanged = base.LoadPostData(postDataKey, postCollection);
            if (controlsStateChanged)
            {
                SetTypedValue();
            }
            return controlsStateChanged;
        }

        private void SetTypedValue()
        {
            _typedValue = string.IsNullOrEmpty(Value) ? null : Json.Deserialize<T>(Value);
        }

        private void EnsureValue()
        {
            if (!_isValueSerialized)
            {
                SerializeValue();
            }
        }

        private void SerializeValue()
        {
            Value = _typedValue == null ? string.Empty : Json.Serialize(_typedValue);
            _isValueSerialized = true;
        }

        protected override void TrackViewState()
        {
            EnsureValue();
            base.TrackViewState();
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            EnsureValue();
            base.RenderControl(writer);
        }

    }
}
