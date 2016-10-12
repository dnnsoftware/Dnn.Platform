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

class EditExtension extends Component {
    render() {
        const {props} = this;
        const {extensionBeingEdited} = props;
        /* eslint-disable react/no-danger */
        return (
            <GridCell style={{ padding: 50 }} className="extension-license">
                {!props.readOnly && <MultiLineInputWithError label="License" tooltipMessage="hey" style={inputStyle} value={extensionBeingEdited.license} onChange={props.onChange.bind(this, "license")} />}
                {props.readOnly &&
                    <Scrollbars style={licenseBoxStyle}>
                        <div className="read-only-license" dangerouslySetInnerHTML={{ __html: extensionBeingEdited.license }}></div>
                    </Scrollbars>
                }
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary" onClick={props.onUpdateExtension.bind(this)}>{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

EditExtension.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default EditExtension;