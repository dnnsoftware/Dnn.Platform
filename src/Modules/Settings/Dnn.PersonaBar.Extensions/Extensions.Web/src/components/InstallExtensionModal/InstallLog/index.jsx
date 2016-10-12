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
        /* eslint-disable react/no-danger */
        return (
            <GridCell style={{ padding: 50 }} className="extension-install-logs">
                <Scrollbars style={licenseBoxStyle}>
                    <div className="package-installation-report">
                        {props.logs.map((log) => {
                            return <p>{log}</p>;
                        })}
                    </div>
                </Scrollbars>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="primary" onClick={props.onDone.bind(this)}>Done</Button>
                </GridCell>
            </GridCell>
        );
    }
}

EditExtension.PropTypes = {
    onDone: PropTypes.func,
    logs: PropTypes.array
};

export default EditExtension;