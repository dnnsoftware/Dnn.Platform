import React, { } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import MultiLineInput from "dnn-multi-line-input";
import Localization from "localization";

const StepFour = ({onCancel, onNext, onChange, selectedManifest, onPrevious}) => (
    <GridCell className="review-manifest-step">
        <h6 className="box-title">{Localization.get("CreatePackage_CreateManifest.Label")}</h6>
        <p className="box-subtitle">{Localization.get("CreatePackage_CreateManifest.HelpText")}</p>
        <GridCell className="package-manifest-container no-padding">
            <MultiLineInput
                className="package-manifest"
                value={selectedManifest}
                onChange={onChange.bind(this, "selectedManifest")} />
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>{Localization.get("Cancel.Button")}</Button>
            <Button type="secondary" onClick={onPrevious}>{Localization.get("Previous.Button")}</Button>
            <Button type="primary" onClick={onNext}>{Localization.get("Next.Button")}</Button>
        </GridCell>
    </GridCell>
);

StepFour.propTypes = {
    onCancel: PropTypes.func,
    onNext: PropTypes.func,
    onChange: PropTypes.func,
    selectedManifest: PropTypes.string,
    onPrevious: PropTypes.func
};
export default StepFour;
