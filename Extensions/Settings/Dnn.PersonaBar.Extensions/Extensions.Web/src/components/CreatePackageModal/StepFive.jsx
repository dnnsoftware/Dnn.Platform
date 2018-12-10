import React, { } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import GridSystem from "dnn-grid-system";
import Localization from "localization";

const inputStyle = { width: "100%" };

const StepFive = ({
    onNext,
    onCancel,
    onChange,
    createPackage,
    createManifest,
    manifestName,
    useExistingManifest,
    archiveName,
    onPrevious
}) => (
    <GridCell className="create-package-step">
        <h6 className="box-title">{Localization.get("CreatePackage_CreatePackage.Label")}</h6>
        <p>{Localization.get("CreatePackage_FinalStep.HelpText")}</p>
        <p>{Localization.get("CreatePackage_FinalStep.HelpTextTwo")}</p>
        {!useExistingManifest && <GridSystem className="with-right-border top-half">
            <div className="left-side">
                <Switch
                    className="create-manifest-switch"
                    label={Localization.get("CreatePackage_CreateManifestFile.Label")}
                    onText={Localization.get("SwitchOn")}
                    offText={Localization.get("SwitchOff")}
                    onChange={onChange.bind(this, "createManifest")}
                    value={createManifest} />
                <SingleLineInputWithError
                    label={Localization.get("CreatePackage_ManifestFileName.Label")}
                    tooltipMessage={Localization.get("CreatePackage_ManifestFileName.HelpText")}
                    style={inputStyle}
                    onChange={onChange.bind(this, "manifestName")}
                    className="extension-package-name"
                    value={manifestName} />
            </div>
            <div className="right-side">
                <Switch
                    className="review-manifest-switch"
                    label={Localization.get("CreatePackage_CreatePackage.Label")}
                    onText={Localization.get("SwitchOn")}
                    offText={Localization.get("SwitchOff")}
                    onChange={onChange.bind(this, "createPackage")}
                    value={createPackage} />
                <SingleLineInputWithError
                    label={Localization.get("CreatePackage_ArchiveFileName.Label")}
                    tooltipMessage={Localization.get("CreatePackage_ArchiveFileName.HelpText")}
                    style={inputStyle}
                    className="extension-package-name"
                    onChange={onChange.bind(this, "archiveName")}
                    value={archiveName} />
            </div>
        </GridSystem>
        }
        {useExistingManifest &&
            <GridCell className="no-padding using-existing-manifest">
                <Switch
                    className="review-manifest-switch"
                    label={Localization.get("CreatePackage_CreatePackage.Label")}
                    onText={Localization.get("SwitchOn")}
                    offText={Localization.get("SwitchOff")}
                    onChange={onChange.bind(this, "createPackage")}
                    value={createPackage} />
                <SingleLineInputWithError
                    label={Localization.get("CreatePackage_ArchiveFileName.Label")}
                    tooltipMessage={Localization.get("CreatePackage_ArchiveFileName.HelpText")}
                    style={inputStyle}
                    className="extension-package-name"
                    onChange={onChange.bind(this, "archiveName")}
                    value={archiveName} />
            </GridCell>
        }
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>{Localization.get("Cancel.Button")}</Button>
            <Button type="secondary" onClick={onPrevious}>{Localization.get("Previous.Button")}</Button>
            <Button type="primary" disabled={!createPackage && !createManifest} onClick={onNext}>{Localization.get("Next.Button")}</Button>
        </GridCell>
    </GridCell>
);

StepFive.propTypes = {
    onNext: PropTypes.func,
    onCancel: PropTypes.func,
    onChange: PropTypes.func,
    createPackage: PropTypes.bool,
    createManifest: PropTypes.bool,
    manifestName: PropTypes.string,
    useExistingManifest: PropTypes.bool,
    archiveName: PropTypes.string,
    onPrevious: PropTypes.func
};
export default StepFive;