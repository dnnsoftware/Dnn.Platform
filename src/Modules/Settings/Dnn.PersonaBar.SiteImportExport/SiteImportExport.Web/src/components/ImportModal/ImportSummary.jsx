import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import Switch from "dnn-switch";

class ImportSummary extends Component {
    getSummaryItem(category) {
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category === category.toUpperCase());
        if (category === "Templates" && !detail) {
            detail = props.importSummary.SummaryItems.find(c => c.Category === "DNN_TEMPLATES");
        }
        return detail ? detail.TotalItems : "-";
    }

    onChange(e) {
        this.props.onSwitchChange(e);
    }

    render() {
        const { props } = this;
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
                                        label={Localization.get("Pages")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Pages")}</div>
                                </GridCell>
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
                                        label={Localization.get("Vocabularies")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Vocabularies")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeProfileProperties")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeProfileProperties.toString()}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludePermissions")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludePermissions.toString()}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("IncludeExtensions")}
                                    />
                                    <div className="import-summary-item">{props.importSummary.IncludeExtensions.toString()}</div>
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
                                    <div className="import-summary-item">{props.selectedPackage.ExporTime}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("ModulePackages")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Extensions")}</div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Assets")}
                                    />
                                    <div className="import-summary-item">{this.getSummaryItem("Assets")}</div>
                                </GridCell>
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
                                        {props.selectedPackage.ExportMode === 1 ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}
                                    </div>
                                </GridCell>
                                <GridCell>
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("LastExport")}
                                    />
                                    <div className="import-summary-item">{props.lastExportTime || "-- --"}</div>
                                </GridCell>
                                <div className="seperator">
                                    <hr />
                                </div>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("OverwriteCollisions")}
                                />
                                <Switch
                                    value={props.collisionResolution === 0 ? false : true}
                                    onChange={this.onChange.bind(this)}
                                />
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