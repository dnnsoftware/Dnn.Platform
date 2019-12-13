#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormToggleButtonItem : DnnFormItemBase
    {
        #region CheckBoxMode enum

        public enum CheckBoxMode
        {
            TrueFalse = 0,
            YN = 1,
            YesNo = 2
        }

        #endregion

        //private DnnRadButton _checkBox;
        private CheckBox _checkBox;

        public DnnFormToggleButtonItem()
        {
            Mode = CheckBoxMode.TrueFalse;
        }

        public CheckBoxMode Mode { get; set; }

        private void CheckedChanged(object sender, EventArgs e)
        {
            string newValue;
            switch (Mode)
            {
                case CheckBoxMode.YN:
                    newValue = (_checkBox.Checked) ? "Y" : "N";
                    break;
                case CheckBoxMode.YesNo:
                    newValue = (_checkBox.Checked) ? "Yes" : "No";
                    break;
                default:
                    newValue = (_checkBox.Checked) ? "true" : "false";
                    break;
            }
            UpdateDataSource(Value, newValue, DataField);
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            //_checkBox = new DnnRadButton {ID = ID + "_CheckBox", ButtonType = RadButtonType.ToggleButton, ToggleType = ButtonToggleType.CheckBox, AutoPostBack = false};
            _checkBox = new CheckBox{ ID = ID + "_CheckBox", AutoPostBack = false };

            _checkBox.CheckedChanged += CheckedChanged;
            container.Controls.Add(_checkBox);

            //Load from ControlState
            if (!_checkBox.Page.IsPostBack)
            {
            }
            switch (Mode)
            {
                case CheckBoxMode.YN:
                case CheckBoxMode.YesNo:
                    var stringValue = Value as string;
                    if (stringValue != null)
                    {
                        _checkBox.Checked = stringValue.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                default:
                    _checkBox.Checked = Convert.ToBoolean(Value);
                    break;
            }

            return _checkBox;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            FormMode = DnnFormMode.Short;
        }
    }
}
