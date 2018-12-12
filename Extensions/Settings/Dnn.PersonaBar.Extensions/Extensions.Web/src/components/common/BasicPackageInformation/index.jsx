import React, { } from "react";
import PropTypes from "prop-types";
import { DropdownWithError, GridSystem, SingleLineInputWithError, Dropdown, MultiLineInputWithError } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import { getVersionDropdownValues, formatVersionNumber } from "utils/helperFunctions";
import styles from "./style.less";

const inputStyle = { width: "100%" };

const BasicPackageInformation = ({disabled, validationMapped, installedPackageTypes, extensionData, onVersionChange, onChange, triedToSave, version, onPackageTypeSelect, isAddMode}) => (
    <GridSystem className={styles.basicPackageInformation + " with-right-border top-half"}>
        <div>
            <DropdownWithError
                className="extension-type"
                options={installedPackageTypes && installedPackageTypes.map((_package) => {
                    return {
                        label: _package.Type.split("_").join("").split(/(?=[A-Z])/).join(" "),
                        value: _package.Type
                    };
                })}
                enabled={!disabled}
                tooltipMessage={Localization.get("EditExtension_PackageType.HelpText")}
                label={Localization.get("EditExtension_PackageType.Label")}
                onSelect={onPackageTypeSelect}
                value={!validationMapped ? extensionData.packageType : (extensionData.packageType.value)}
                style={inputStyle} />
            <SingleLineInputWithError
                label={Localization.get("EditExtension_PackageName.Label")}
                tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                style={inputStyle}
                enabled={isAddMode}
                onChange={onChange && onChange.bind(this, "name")}
                className="extension-package-name"
                value={!validationMapped ? extensionData.name : extensionData.name.value} />
            <SingleLineInputWithError
                label={Localization.get("EditExtension_PackageFriendlyName.Label") + "*"}
                tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                value={!validationMapped ? extensionData.friendlyName : extensionData.friendlyName.value}
                style={inputStyle}
                className="extension-package-friendly-name"
                error={extensionData.friendlyName.error && triedToSave}
                enabled={!disabled}
                onChange={onChange && onChange.bind(this, "friendlyName")} />
        </div>
        <div>
            <MultiLineInputWithError
                label={Localization.get("EditExtension_PackageDescription.Label")}
                tooltipMessage={Localization.get("EditExtension_PackageDescription.HelpText")}
                style={inputStyle}
                className="extension-description"
                inputStyle={{ marginBottom: 28, height: 123 }}
                value={!validationMapped ? extensionData.description : extensionData.description.value}
                enabled={!disabled}
                onChange={onChange && onChange.bind(this, "description")} />
            <DropdownWithError
                options={getVersionDropdownValues()}
                label={Localization.get("EditExtension_PackageVersion.Label")}
                tooltipMessage={Localization.get("EditExtension_PackageVersion.HelpText")}
                enabled={!disabled}
                defaultDropdownValue={formatVersionNumber(version[0])}
                onSelect={onVersionChange && onVersionChange.bind(this, 0)}
                className="version-dropdown"
                style={{position: "relative", top: 3}} />
            <Dropdown
                options={getVersionDropdownValues()}
                className="version-dropdown"
                label={formatVersionNumber(version[1])}
                onSelect={onVersionChange && onVersionChange.bind(this, 1)}
                enabled={!disabled} />
            <Dropdown
                options={getVersionDropdownValues()}
                label={formatVersionNumber(version[2])}
                className="version-dropdown"
                onSelect={onVersionChange && onVersionChange.bind(this, 2)}
                enabled={!disabled} />
        </div>
    </GridSystem>
);

BasicPackageInformation.propTypes = {
    disabled: PropTypes.bool,
    validationMapped: PropTypes.bool,
    installedPackageTypes: PropTypes.array,
    extensionData: PropTypes.object,
    onVersionChange: PropTypes.func,
    onChange: PropTypes.func,
    triedToSave: PropTypes.bool,
    version: PropTypes.array,
    onPackageTypeSelect: PropTypes.func,
    isAddMode: PropTypes.bool
};

export default BasicPackageInformation;