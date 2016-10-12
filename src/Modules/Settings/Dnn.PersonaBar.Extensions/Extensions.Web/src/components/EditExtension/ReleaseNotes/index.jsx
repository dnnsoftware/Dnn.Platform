import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Button from "dnn-button";

const inputStyle = { width: "100%" };
class EditExtension extends Component {
    render() {
        const {props} = this;
        const {extensionBeingEdited} = props;
        return (
            <GridCell style={{ padding: 50 }}>
                <MultiLineInputWithError label="Release Notes" tooltipMessage="hey" style={inputStyle} value={extensionBeingEdited.releaseNotes} onChange={props.onChange.bind(this, "releaseNotes")} />
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