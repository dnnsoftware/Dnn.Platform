import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, Button, Tooltip } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
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
        let errorCount = props.logs.filter((log) => { return log.Type === "Failure" || log.Type === "Error"; }).length;
        /* eslint-disable react/no-danger */
        return (
            <GridCell style={{ padding: 50 }} className={"extension-install-logs" + (errorCount > 0 ? " with-error" : "")}>
                <h6>{Localization.get("InstallExtension_Logs.Header")}</h6>
                <Tooltip messages={[Localization.get("InstallationError")]} type="error" rendered={errorCount > 0} className="install-error-tooltip" />
                <p>{Localization.get("InstallExtension_Logs.HelpText")}</p>
                <Scrollbars style={errorCount > 0 ? Object.assign({borderBottom: "2px solid #EA2134"}, licenseBoxStyle) : licenseBoxStyle}>
                    <div className="package-installation-report">
                        {props.logs.map((log, i) => {
                            return <p className={log.Type.toLowerCase()} key={i}>{log.Type + " " + log.Description}</p>;
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

EditExtension.propTypes = {
    onDone: PropTypes.func,
    logs: PropTypes.array
};

export default EditExtension;