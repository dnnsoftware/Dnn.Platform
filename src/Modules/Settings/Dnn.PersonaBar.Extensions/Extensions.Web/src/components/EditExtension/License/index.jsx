import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import { Scrollbars } from "react-custom-scrollbars";
import Button from "dnn-button";
import "./style.less";

const inputStyle = { width: "100%" };
const licenseBoxStyle = {
    border: "1px solid #c8c8c8",
    marginBottom: 25,
    height: 250,
    width: "100%"
};

class License extends Component {
    render() {
        const {props} = this;
        const {value} = props;
        /* eslint-disable react/no-danger */
        return (
            <GridCell style={{ padding: 50 }} className="extension-license extension-form">
                {!props.readOnly &&
                    <MultiLineInputWithError
                        label="License"
                        style={inputStyle}
                        value={value}
                        enabled={!props.disabled}
                        onChange={props.onChange && props.onChange.bind(this, "license")} />}
                {props.readOnly &&
                    <Scrollbars style={licenseBoxStyle}>
                        <div className="read-only-license" dangerouslySetInnerHTML={{ __html: value }}></div>
                    </Scrollbars>
                }
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>Save & Close</Button>}
                    <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

License.PropTypes = {
    onCancel: PropTypes.func,
    readOnly: PropTypes.bool,
    onSave: PropTypes.func,
    value: PropTypes.string,
    onChange: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default License;