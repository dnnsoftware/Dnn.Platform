import React from "react";
import PropTypes from "prop-types";
import { GridCell, Button } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";

const packageCreationStyle = {
    border: "1px solid #c8c8c8",
    marginBottom: 25,
    height: 250,
    width: "100%"
};

const StepSix = ({onClose, logs}) => (
    <GridCell className="review-logs-step">
        <h6 className="box-title">{Localization.get("CreatePackage_ChooseFiles.Label")}</h6>
        <p className="box-subtitle">{Localization.get("CreatePackage_ChooseFiles.HelpText")}</p>
        <GridCell className="package-logs-container no-padding">

            <Scrollbars style={packageCreationStyle}>
                <div className="package-creation-report">
                    {logs.map((log, i) => {
                        return <p key={i}>{log}</p>;
                    })}
                </div>
            </Scrollbars>
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="primary" onClick={onClose}>{Localization.get("Done.Button")}</Button>
        </GridCell>
    </GridCell>
);

StepSix.propTypes = {
    onClose: PropTypes.func,
    logs: PropTypes.array
};
export default StepSix;
