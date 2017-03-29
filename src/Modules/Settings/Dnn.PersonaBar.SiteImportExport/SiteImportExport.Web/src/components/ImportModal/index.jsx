import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions,
    visiblePanel as VisiblePanelActions
} from "../../actions";
import { TemplateIcon, CycleIcon, CheckMarkIcon } from "dnn-svg-icons";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";
import PackageCardOverlay from "./PackageCardOverlay";
import Button from "dnn-button";
import Label from "dnn-label";
import Switch from "dnn-switch";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
import util from "utils";

class ImportModal extends Component {
    constructor() {
        super();
        this.state = {
            wizardStep: 0,
            importRequest: {
                PortalId: util.settings.portalId,
                PackageId: -1,
                CollisionResolution: 1
            }
        };
    }

    componentWillMount() {
        const { props } = this;
        props.dispatch(ImportExportActions.getImportPackages());
    }

    componentWillReceiveProps(props) {
        this.setState({
            wizardStep: props.wizardStep
        });
    }

    goToStep(wizardStep) {
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
        props.dispatch(ImportExportActions.importWizardGoToSetp(0, () => {
            props.dispatch(ImportExportActions.selectPackage());
        }));
    }

    selectPackage(pkg) {
        const { props } = this;
        props.dispatch(ImportExportActions.selectPackage(pkg, () => {
            let { importRequest } = this.state;
            importRequest.PackageId = pkg.PackageId;
            this.setState({
                importRequest
            });
        }));
    }

    cancelImport() {
        const { props } = this;
        if (props.wizardStep === 0) {
            this.goToStep(0);
            props.onCancel();
        }
        else {
            props.dispatch(ImportExportActions.importWizardGoToSetp(0, () => {
                props.dispatch(ImportExportActions.selectPackage());
            }));
        }
    }

    onAnalyze() {
        const { props } = this;
        if (props.selectedPackage) {
            props.dispatch(ImportExportActions.importWizardGoToSetp(1, () => {
                props.dispatch(ImportExportActions.verifyImportPackage(props.selectedPackage.PackageId));
            }));
        }
        else {
            util.utilities.notifyError(Localization.get("SelectException"));
        }
    }

    onImport() {
        const { props, state } = this;
        props.dispatch(ImportExportActions.importSite(state.importRequest, (data) => {
            util.utilities.notify(Localization.get("ImportRequestSubmitted"));
            this.goToStep(0);
            props.dispatch(ImportExportActions.getAllJobs({
                portalId: state.importRequest.PortalId,
                pageIndex: 0,
                pageSize: 10
            }));
            props.dispatch(VisiblePanelActions.selectPanel(0));
        }, () => {
            util.utilities.notifyError(Localization.get("ImportRequestSubmit.ErrorMessage"));
        }));
    }

    /* eslint-disable react/no-danger */
    renderTemplateThumbnail(imgData) {
        if (imgData) {
            return <img className="package-image" src={"data:image/jpeg;base64," + btoa(imgData)}></img>;
        }
        else {
            return <div className="template-icon" dangerouslySetInnerHTML={{ __html: TemplateIcon }}></div>;
        }
    }

    renderPackagesList() {
        const { props } = this;
        if (props.importPackages && props.importPackages.length > 0) {
            return <Scrollbars
                className="package-card-scroller"
                autoHeight
                autoHeightMin={0}
                autoHeightMax={210}>
                <ul style={{ width: 200, height: props.importPackages.length > 3 ? 210 : "auto" }}>
                    {props.importPackages.map((pkg, index) => {
                        return <div className={(props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) ? "package-card selected" : "package-card"}>
                            <div id={"package-card-" + index}>
                                {this.renderTemplateThumbnail()}
                                <div className="package-name">{pkg.Name}</div>
                                <div className="package-file">{pkg.FileName}</div>
                            </div>
                            <PackageCardOverlay selectPackage={this.selectPackage.bind(this, pkg)} packageName={pkg.Name} packageDescription={pkg.Description} />
                        </div>;
                    })}
                </ul>
            </Scrollbars>;
        }
        else return <div className="noPackages">{Localization.get("NoPackages")}</div>;
    }

    renderPackageVerification() {
        const { props } = this;
        return <div>
            {props.selectedPackage && <div className="package-analyzing">
                <div className="package-file">{props.selectedPackage.FileName}</div>
                <div className="analyzing-wrapper">
                    <div className={props.importSummary ? "analyzed-icon" : "analyzing-icon"}
                        dangerouslySetInnerHTML={{ __html: props.importSummary ? CheckMarkIcon : CycleIcon }}>
                    </div>
                    <div className="analyzing-message">{props.importSummary ? Localization.get("AnalyzedPackage") : Localization.get("AnalyzingPackage")}</div>
                </div>
            </div>}
        </div>;
    }

    getSummaryItem(category) {
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category === category.toUpperCase());
        return detail ? detail.TotalItems : "-";
    }

    onSwitchChange(event) {
        const value = typeof event === "object" ? event.target.value : event;
        let { importRequest } = this.state;
        importRequest.CollisionResolution = value ? 1 : 0;
        this.setState({
            importRequest
        });
    }

    renderImportSummary() {
        const { props, state } = this;
        return <div style={{ float: "left", width: "100%" }}>
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
                                <div className="import-summary-item">{this.getSummaryItem("PageTemplates")}</div>
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
                                    label={Localization.get("FileName")}
                                />
                                <div className="import-summary-item">{props.selectedPackage.FileName}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Timestamp")}
                                />
                                <div className="import-summary-item">{props.importSummary.Timestamp}</div>
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
                                <div className="import-summary-item">{props.selectedPackage.TotalExportSize}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ExportMode")}
                                />
                                <div className="import-summary-item">{props.selectedPackage.ExportMode === 1 ? Localization.get("ExportModeDifferential") : Localization.get("ExportModeComplete")}</div>
                            </GridCell>
                            <GridCell>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("LastExport")}
                                />
                                <div className="import-summary-item">{props.selectedPackage.LastExport}</div>
                            </GridCell>
                            <div className="seperator">
                                <hr />
                            </div>
                            <Label
                                labelType="inline"
                                label={Localization.get("OverwriteCollisions")}
                            />
                            <Switch
                                value={state.importRequest.CollisionResolution === 0 ? false : true}
                                onChange={this.onSwitchChange.bind(this)}
                            />
                        </div>
                    </GridCell>
                    <div className="finish-importing">{Localization.get("FinishImporting")}</div>
                </div>
            }
        </div>;
    }

    render() {
        const { props, state } = this;
        return (
            <div className={styles.importModal}>
                <div className="pageTitle">{Localization.get("SelectImportPackage.Header")}</div>
                <div className="pageDescription">{Localization.get("SelectImportPackage")}</div>
                <div className="packages-wrapper">
                    <div className="packages">
                        {state.wizardStep === 0 &&
                            this.renderPackagesList()
                        }
                        {state.wizardStep === 1 &&
                            this.renderPackageVerification()
                        }
                    </div>
                    {state.wizardStep === 1 &&
                        this.renderImportSummary()
                    }
                </div>
                <GridCell className="action-buttons">
                    <Button type="secondary" onClick={this.cancelImport.bind(this)}>{Localization.get("Cancel")}</Button>
                    {props.wizardStep === 0 &&
                        <Button type="primary" disabled={props.selectedPackage ? false : true} onClick={this.onAnalyze.bind(this)}>{Localization.get("Continue")}</Button>
                    }
                    {props.wizardStep === 1 &&
                        <Button type="primary" disabled={!props.importSummary} onClick={this.onImport.bind(this)}>{Localization.get("Continue")}</Button>
                    }
                </GridCell>
            </div>
        );
    }
}

ImportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    importPackages: PropTypes.array,
    selectedPackage: PropTypes.object,
    importSummary: PropTypes.object
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.importWizardStep,
        importPackages: state.importExport.importPackages,
        selectedPackage: state.importExport.selectedPackage,
        importSummary: state.importExport.importSummary
    };
}

export default connect(mapStateToProps)(ImportModal);