import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import { Scrollbars } from "react-custom-scrollbars";
import Button from "dnn-button";
import Localization from "localization";
import "./style.less";

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
                <h6>{Localization.get("InstallExtension_Logs.Header")}</h6>
                <p>{Localization.get("InstallExtension_Logs.HelpText")}</p>
                <Scrollbars style={licenseBoxStyle}>
                    <div className="package-installation-report">
                        {props.logs.map((log) => {
                            return <p>{log}</p>;
                        })}
                    </div>
                </Scrollbars>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="primary" onClick={props.onDone.bind(this)}>{Localization.get("Done.Button")}</Button>
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