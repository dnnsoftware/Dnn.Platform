import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";
import "./style.less";

const licenseBoxStyle = {
    border: "1px solid #c8c8c8",
    borderBottom: "2px solid #EA2134",
    marginBottom: 25,
    height: 175,
    width: "100%"
};

class LogDisplay extends Component {
    render() {
        const {props} = this;
        /* eslint-disable react/no-danger */
        return (
            <GridCell style={{ padding: 0 }} className="install-failure-logs">
                <Scrollbars style={licenseBoxStyle}>
                    <div className="package-installation-report">
                        {props.logs && props.logs.map((log, i) => {
                            return <p className={log.Type.toLowerCase()} key={i}>{log.Type + " " + log.Description}</p>;
                        })}
                        {!props.logs && <p className="logs-unknown-error" dangerouslySetInnerHTML={{ __html: Localization.get("InstallExtension_UploadFailedUnknownLogs") }}></p>}
                    </div>
                </Scrollbars>
            </GridCell>
        );
    }
}

LogDisplay.propTypes = {
    onDone: PropTypes.func,
    logs: PropTypes.array
};

export default LogDisplay;
