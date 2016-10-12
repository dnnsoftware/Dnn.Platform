import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import { Scrollbars } from "react-custom-scrollbars";
import Button from "dnn-button";
import styles from "./style.less";

const inputStyle = { width: "100%" };
const releaseBoxStyle = {
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
            <GridCell style={{ padding: 50 }} className="release-notes extension-form">
                {!props.readOnly && <MultiLineInputWithError
                    label="Release Notes"
                    style={inputStyle}
                    value={extensionBeingEdited.releaseNotes}
                    onChange={props.onChange && props.onChange.bind(this, "releaseNotes")} />}
                {props.readOnly &&
                    <Scrollbars style={releaseBoxStyle}>
                        <div className="read-only-release-notes" dangerouslySetInnerHTML={{ __html: extensionBeingEdited.releaseNotes }}></div>
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
    readOnly: PropTypes.bool,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default EditExtension;