import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import BasicPackageInformation from "../common/BasicPackageInformation";
import Switch from "dnn-switch";
import Button from "dnn-button";
import DropdownWithError from "dnn-dropdown-with-error";
import MultiLineInput from "dnn-multi-line-input";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Localization from "localization";

const StepTwo = ({packageManifest, onCancel, onNext, onBasePathChange, onPrevious, onFileOrAssemblyChange, onRefresh}) => (
    <GridCell className="review-files-step">
        <h6 className="box-title">{Localization.get("CreatePackage_ChooseFiles.Label")}</h6>
        <p className="box-subtitle">{Localization.get("CreatePackage_ChooseFiles.HelpText")}</p>
        <GridCell className="no-padding create-package-folder">
            <GridCell columnSize={70} className="no-padding create-package-folder-input-container">
                <SingleLineInputWithError
                    value={packageManifest.basePath}
                    onChange={onBasePathChange.bind(this)}
                    className="create-package-folder-input"
                    inputStyle={{ marginBottom: 0 }}
                    label={Localization.get("CreatePackage_Folder.Label")}
                    />
            </GridCell>
            <GridCell columnSize={30} className="no-padding refresh-file-list-button">
                <Button type="secondary" onClick={onRefresh}>{Localization.get("CreatePackage_RefreshFileList.Button")}</Button>
            </GridCell>
        </GridCell>
        <GridCell className="package-files-container no-padding">
            <MultiLineInput
                className="package-files"
                value={packageManifest.files.join("\n")}
                onChange={onFileOrAssemblyChange.bind(this, "files")}
                />
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>Cancel</Button>
            <Button type="secondary" onClick={onPrevious}>Previous</Button>
            <Button type="primary" onClick={onNext}>Next</Button>
        </GridCell>
    </GridCell>
);

StepTwo.propTypes = {
    packageManifest: PropTypes.object,
    onCancel: PropTypes.func,
    onNext: PropTypes.func,
    onBasePathChange: PropTypes.func,
    onPrevious: PropTypes.func,
    onFileOrAssemblyChange: PropTypes.func,
    onRefresh: PropTypes.func
};
export default StepTwo;