import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import BasicPackageInformation from "../common/BasicPackageInformation";
import Switch from "dnn-switch";
import Button from "dnn-button";
import MultiLineInput from "dnn-multi-line-input";
import Localization from "localization";

const StepFour = ({packageManifest, onCancel, onNext, onChange, selectedManifest, onPrevious}) => (
    <GridCell className="review-manifest-step">
        <h6 className="box-title">{Localization.get("CreatePackage_CreateManifest.Label")}</h6>
        <p className="box-subtitle">{Localization.get("CreatePackage_CreateManifest.HelpText")}</p>
        <GridCell className="package-manifest-container no-padding">
            <MultiLineInput
                className="package-manifest"
                value={selectedManifest}
                onChange={onChange.bind(this, "selectedManifest")}
                />
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>Cancel</Button>
            <Button type="secondary" onClick={onPrevious}>Previous</Button>
            <Button type="primary" onClick={onNext}>Next</Button>
        </GridCell>
    </GridCell>
);

StepFour.propTypes = {
    packageManifest: PropTypes.object,
    onCancel: PropTypes.func,
    onNext: PropTypes.func,
    onChange: PropTypes.func,
    selectedManifest: PropTypes.string,
    onPrevious: PropTypes.func
};
export default StepFour;