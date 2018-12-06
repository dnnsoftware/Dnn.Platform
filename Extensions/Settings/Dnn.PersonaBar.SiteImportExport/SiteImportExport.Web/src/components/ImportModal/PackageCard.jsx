import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";

class PackageCard extends Component {
    render() {
        const { props } = this;
        return (
            <div className={props.className || "package-card"}>
                <div className="card-grid">
                    <GridCell columnSize={35} className="card-column1">
                        <div className="package-name">
                            <TextOverflowWrapper text={props.selectedPackage.Name} maxWidth={200} />
                        </div>
                        <div className="package-field">
                            <TextOverflowWrapper text={props.selectedPackage.ExporTimeString} maxWidth={200} />
                        </div>
                    </GridCell>
                    <GridCell columnSize={17} className="card-column2">
                        <div className="package-field">
                            <Label
                                labelType="inline"
                                label={Localization.get("FolderName")} />
                        </div>
                        <div className="package-field">
                            <Label
                                labelType="inline"
                                label={Localization.get("Website")} />
                        </div>
                    </GridCell>
                    <GridCell columnSize={23} className="card-column3">
                        <div className="package-field">
                            <TextOverflowWrapper text={props.selectedPackage.FileName} maxWidth={160} />
                        </div>
                        <div className="package-field">
                            <TextOverflowWrapper text={props.selectedPackage.PortalName} maxWidth={160} />
                        </div>
                    </GridCell>
                    <GridCell columnSize={12} className="card-column4">
                        <div className="package-field">
                            <Label
                                labelType="inline"
                                label={Localization.get("Mode")} />
                        </div>
                        <div className="package-field">
                            <Label
                                labelType="inline"
                                label={Localization.get("FileSize")} />
                        </div>
                    </GridCell>
                    <GridCell columnSize={13} className="card-column5">
                        <div className="package-field">
                            <TextOverflowWrapper
                                text={props.selectedPackage.Summary.ExportMode ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}
                                maxWidth={70} />
                        </div>
                        <div className="package-field">
                            <TextOverflowWrapper text={((props.selectedPackage.Summary || {}).ExportFileInfo || {}).ExportSize} maxWidth={70} />
                        </div>
                    </GridCell>
                </div>
                {props.children}
            </div>
        );
    }
}

PackageCard.propTypes = {
    selectedPackage: PropTypes.object,
    children: PropTypes.node,
    className: PropTypes.string
};

export default PackageCard;