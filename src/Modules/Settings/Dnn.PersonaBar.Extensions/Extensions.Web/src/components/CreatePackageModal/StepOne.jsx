import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import BasicPackageInformation from "../common/BasicPackageInformation";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";

const StepOne = ({extensionBeingEdited, version, installedPackageTypes, onNext}) => (
    <GridCell className="package-manifest-step">
    <BasicPackageInformation
        extensionData={extensionBeingEdited}
        readOnly={true}
        version={version}
        installedPackageTypes={installedPackageTypes} />
    <GridCell className="package-manifest-info">
        <hr />
        <h6 className="package-manifest-header">{Localization.get("CreatePackage_PackageManifest.Header")}</h6>
        <p className="package-manifest-help-text">{Localization.get("CreatePackage_PackageManifest.HelpText")}</p>
        <Switch
            className="existing-manifest-switch"
            label={Localization.get("CreatePackage_UseExistingManifest.Label")}
            value={true}
            />
    </GridCell>
    <GridCell className="modal-footer">
        <Button type="secondary">Cancel</Button>
        <Button type="primary" onClick={onNext.bind(this)}>Next</Button>
    </GridCell>
</GridCell>
);

StepOne.propTypes = {
    extensionBeingEdited: PropTypes.object,
    version: PropTypes.array,
    installedPackageTypes: PropTypes.array,
    onNext: PropTypes.func
};
export default StepOne;