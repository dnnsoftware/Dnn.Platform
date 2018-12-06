import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import Switch from "dnn-switch";
import itemsToExportService from "../../services/itemsToExportService";

class ImportSummary extends Component {
    getSummaryItem(category) {
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category.toUpperCase() === category.toUpperCase());
        return detail ? detail.TotalItemsString : "-";
    }

    onChange(key, e) {
        this.props.onSwitchChange(key, e);
    }

    render() {
        const { props } = this;
        const registeredItemsToExport = itemsToExportService.getRegisteredItemsToExport();
        return (
            <div style={{ float: "left", width: "100%" }}>
                {props.importSummary && props.selectedPackage &&
                    <div className="import-summary">
                        <div className="sectionTitle">{Localization.get("ImportSummary")}</div>
                        <GridCell className="import-site-container">
                            <div className="left-column">
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Users")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Users")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Pages")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Pages")}</div>
                                </GridCell>                                
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Roles")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Roles")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Vocabularies")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Vocabularies")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("PageTemplates")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Templates")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeContent")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeContent ? Localization.get("Yes") : Localization.get("No")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeProfileProperties")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeProfileProperties ? Localization.get("Yes") : Localization.get("No")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludePermissions")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludePermissions ? Localization.get("Yes") : Localization.get("No")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeExtensions")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeExtensions ? Localization.get("Yes") : Localization.get("No")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeDeletions")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeDeletions ? Localization.get("Yes") : Localization.get("No")}</div>
                                </GridCell>
                            </div>
                            <div className="right-column">
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("FolderName")}
                                    />
                                    <div className="import-summary-item">{props.selectedPackage.FileName}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Timestamp")}
                                    />
                                    <div className="import-summary-item">{props.selectedPackage.ExporTimeString}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Extensions")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Packages")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Assets")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Assets")}</div>
                                </GridCell>
                                {registeredItemsToExport.map((item, i) =>
                                    <GridCell key={i}>
                                        <Label
                                            labelType="inline"
                                            label={item.name}
                                        />
                                        <div className="import-summary-item">{this.getSummaryItem(item.category)}</div>
                                    </GridCell>)
                                }
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("TotalExportSize")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.ExportFileInfo.ExportSize}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("ExportMode")}
                                    />
                                    <div className="import-summary-item">
                                        {props.importSummary.ExportMode === 1 ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}
                                    </div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("LastExport")}
                                    />
                                    <div className="import-summary-item">{props.lastExportTime || Localization.get("EmptyDateTime")}</div>
                                </GridCell>
                                <div className="seperator">
                                    <hr />
                                </div>
                                <GridCell style={{paddingBottom: "6"}}>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("OverwriteCollisions")}
                                    />
                                    <Switch
                                        onText={Localization.get("SwitchOn")}
                                        offText={Localization.get("SwitchOff")}
                                        value={props.collisionResolution === 0 ? false : true}
                                        onChange={this.onChange.bind(this, "CollisionResolution")}
                                    />
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("RunNow")}
                                    />
                                    <Switch
                                        onText={Localization.get("SwitchOn")}
                                        offText={Localization.get("SwitchOff")}
                                        value={props.runNow}
                                        onChange={this.onChange.bind(this, "RunNow")}
                                    />
                                </GridCell>
                            </div>
                        </GridCell>
                        <div className="finish-importing">{Localization.get("FinishImporting")}</div>
                    </div>
                }
            </div>
        );
    }
}

ImportSummary.propTypes = {
    importSummary: PropTypes.object,
    selectedPackage: PropTypes.object,
    collisionResolution: PropTypes.number,
    runNow: PropTypes.bool,
    onSwitchChange: PropTypes.func,
    lastExportTime: PropTypes.string
};

function mapStateToProps(state) {
    return {
        selectedPackage: state.importExport.selectedPackage,
        importSummary: state.importExport.importSummary,
        lastExportTime: state.importExport.lastExportTime
    };
}

export default connect(mapStateToProps)(ImportSummary);