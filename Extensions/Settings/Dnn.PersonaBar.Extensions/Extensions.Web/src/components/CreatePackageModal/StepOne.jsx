import React, { } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import BasicPackageInformation from "../common/BasicPackageInformation";
import Switch from "dnn-switch";
import Button from "dnn-button";
import DropdownWithError from "dnn-dropdown-with-error";
import Localization from "localization";

const StepOne = ({packageManifest,
    version,
    installedPackageTypes,
    onNext,
    onCancel,
    useExistingManifest,
    onChange,
    hasManifests,
    manifestDropdown,
    selectedManifest,
    onSelect,
    reviewManifest
}) => (
    <GridCell className="package-manifest-step">
        <BasicPackageInformation
            extensionData={packageManifest}
            validationMapped={false}
            disabled={true}
            version={version}
            installedPackageTypes={installedPackageTypes} />
        <GridCell className="package-manifest-info">
            <hr />
            <h6 className="box-title package-manifest-header">{Localization.get("CreatePackage_PackageManifest.Header")}</h6>
            <p className="box-subtitle">{Localization.get("CreatePackage_PackageManifest.HelpText")}</p>
            {hasManifests &&
                <GridCell className="no-padding">
                    <Switch
                        className="existing-manifest-switch"
                        label={Localization.get("CreatePackage_UseExistingManifest.Label")}
                        onText={Localization.get("SwitchOn")}
                        offText={Localization.get("SwitchOff")}
                        onChange={onChange.bind(this, "useExistingManifest")}
                        value={useExistingManifest} />
                </GridCell>
            }
            {useExistingManifest &&
                <GridCell className="no-padding">
                    <DropdownWithError
                        className="existing-manifest-dropdown"
                        options={manifestDropdown}
                        label={Localization.get("CreatePackage_ManifestFile.Label")}
                        onSelect={onSelect.bind(this, "selectedManifest")}
                        value={selectedManifest} />
                </GridCell>
            }
            <GridCell className="no-padding">
                <Switch
                    className="review-manifest-switch"
                    label={Localization.get("CreatePackage_ReviewManifest.Label")}
                    onText={Localization.get("SwitchOn")}
                    offText={Localization.get("SwitchOff")}
                    onChange={onChange.bind(this, "reviewManifest")}
                    value={reviewManifest} />
            </GridCell>
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>{Localization.get("Cancel.Button")}</Button>
            <Button type="primary" onClick={onNext}>{Localization.get("Next.Button")}</Button>
        </GridCell>
    </GridCell>
);

StepOne.propTypes = {
    packageManifest: PropTypes.object,
    version: PropTypes.array,
    installedPackageTypes: PropTypes.array,
    onNext: PropTypes.func,
    onCancel: PropTypes.func,
    useExistingManifest: PropTypes.bool,
    onChange: PropTypes.func,
    hasManifests: PropTypes.bool,
    manifestDropdown: PropTypes.array,
    selectedManifest: PropTypes.bool,
    onSelect: PropTypes.func,
    reviewManifest: PropTypes.bool
};
export default StepOne;