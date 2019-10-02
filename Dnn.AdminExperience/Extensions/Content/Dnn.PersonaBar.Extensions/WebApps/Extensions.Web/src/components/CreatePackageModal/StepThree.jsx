import React, { } from "react";
import PropTypes from "prop-types";
import { GridCell, Button, MultiLineInput } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";

const StepThree = ({packageManifest, onCancel, onNext, onPrevious, onFileOrAssemblyChange}) => (
    <GridCell className="review-assemblies-step">
        <h6 className="box-title">{Localization.get("CreatePackage_ChooseAssemblies.Label")}</h6>
        <p className="box-subtitle">{Localization.get("CreatePackage_ChooseAssemblies.HelpText")}</p>
        <GridCell className="package-assemblies-container no-padding">
            <MultiLineInput
                className="package-assemblies"
                value={packageManifest.assemblies.join("\n")}
                onChange={onFileOrAssemblyChange.bind(this, "assemblies")} />
        </GridCell>
        <GridCell className="modal-footer">
            <Button type="secondary" onClick={onCancel}>{Localization.get("Cancel.Button")}</Button>
            <Button type="secondary" onClick={onPrevious}>{Localization.get("Previous.Button")}</Button>
            <Button type="primary" onClick={onNext}>{Localization.get("Next.Button")}</Button>
        </GridCell>
    </GridCell>
);

StepThree.propTypes = {
    packageManifest: PropTypes.object,
    onCancel: PropTypes.func,
    onNext: PropTypes.func,
    onPrevious: PropTypes.func,
    onFileOrAssemblyChange: PropTypes.func
};
export default StepThree;
