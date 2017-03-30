import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions,
    visiblePanel as VisiblePanelActions
} from "../../actions";
import Localization from "localization";
import InputGroup from "dnn-input-group";
import Label from "dnn-label";
import Dropdown from "dnn-dropdown-with-error";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
import PagePicker from "dnn-page-picker";
import Button from "dnn-button";
import styles from "./style.less";
import utilities from "utils";

const scrollAreaStyle = {
    width: "100%",
    height: 200,
    marginTop: 0,
    border: "1px solid #c8c8c8"
};

const keysToValidate = ["ExportName", "ExportDescription"];

class ExportModal extends Component {
    constructor() {
        super();
        this.state = {
            wizardStep: 0,
            exportRequest: {
                PortalId: -1,
                ExportName: "",
                ExportDescription: "",
                ItemsToExport: ["Assets", "Users", "Roles", "Vocabularies", "Profile_Properties", "Permissions", "PageTemplates"],
                IncludeDeletions: false,
                IncludeContent: true,
                IncludeFiles: true,
                ExportMode: "Differential"
            },
            errors: {
                ExportName: false,
                ExportDescription: false
            }
        };
    }

    componentWillMount() {
        const { props, state } = this;
        const { exportRequest } = state;
        exportRequest.PortalId = props.portalId;
        this.setState({
            exportRequest
        }, () => {
            props.dispatch(ImportExportActions.getLastJobTime({ "portal": props.portalId, "jobType": "Export" }));
        });
    }

    componentWillReceiveProps(props) {
        /*const { state } = this;
        const { exportRequest } = state;
        if (exportRequest.PortalId === -1 || exportRequest.PortalId !== props.portalId) {
            exportRequest.PortalId = props.portalId;
            this.setState({
                exportRequest
            }, () => {
                props.dispatch(ImportExportActions.getLastJobTime({ "portal": props.portalId, "jobType": "Export" }));
            });
        }*/
    }

    goToStep(wizardStep) {
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
    }

    cancelExport() {
        const { props } = this;
        this.goToStep(0);
        props.onCancel();
    }

    onExportPortal() {
        const { props, state } = this;
        if (this.Validate()) {
            props.dispatch(ImportExportActions.exportSite(state.exportRequest, (data) => {
                utilities.utilities.notify(Localization.get("ExportRequestSubmitted"));
                props.dispatch(ImportExportActions.getAllJobs({
                    portal: props.portalId,
                    pageIndex: 0,
                    pageSize: 10
                }));
                props.dispatch(VisiblePanelActions.selectPanel(0));
            }, () => {
                utilities.utilities.notifyError(Localization.get("ExportRequestSubmit.ErrorMessage"));
            }));
        }
    }

    Validate() {
        let success = true;
        const { exportRequest } = this.state;
        success = this.ValidateTexts();
        if (success && exportRequest.pages.length <= 0) {
            success = false;
            utilities.notify(Localization.get("ErrorPages"));
        }
        return success;
    }

    ValidateTexts(key) {
        let success = true;
        const { exportRequest } = this.state;
        const { errors } = this.state;
        keysToValidate.map(vkey => {
            if (key === undefined || key === vkey) {
                if (exportRequest[vkey] === "") {
                    success = false;
                    errors[vkey] = true;
                } else {
                    errors[vkey] = false;
                }
            }
            this.setState({});
        });
        return success;
    }

    getPortalOptions() {
        const { props } = this;
        let options = [{ label: props.portalName, value: props.portalId }];
        return options;
    }

    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let { exportRequest } = this.state;

        if (key === "Assets" ||
            key === "Users" ||
            key === "Roles" ||
            key === "Vocabularies" ||
            key === "Profile_Properties" ||
            key === "Permissions" ||
            key === "PageTemplates") {
            if (value) {
                if (!(exportRequest.ItemsToExport.includes(key))) {
                    exportRequest.ItemsToExport.push(key);
                }
            }
            else {
                exportRequest.ItemsToExport.splice(exportRequest.ItemsToExport.indexOf(key), 1);
            }
        }
        else {
            exportRequest[key] = value;
        }
        this.setState({
            exportRequest
        });
        if (keysToValidate.some(vkey => vkey === key))
            this.ValidateTexts(key);
    }

    updatePagesToExport(selectedPages) {
        let { exportRequest } = this.state;
        exportRequest.pages = selectedPages;
        this.setState({ exportRequest });
    }

    getExportModeOptions() {
        let options = [
            { label: Localization.get("ExportModeDifferential"), value: "Differential" },
            { label: Localization.get("ExportModeComplete"), value: "Complete" }
        ];
        return options;
    }

    render() {
        const { state, props } = this;
        const PortalTabsParameters = {
            portalId: props.portalId,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: true,
            disabledNotSelectable: false,
            roles: "",
            sortOrder: 0
        };
        return (
            <div className={styles.exportModal}>
                <div className="pageTitle">{Localization.get("ExportSettings")}</div>
                <GridCell className="export-site-container">
                    <div className="left-column">
                        <GridCell>
                            <Dropdown
                                enabled={false}
                                label={Localization.get("Site")}
                                options={this.getPortalOptions()}
                                value={props.portalId}
                                onSelect={this.onChange.bind(this, "PortalId")}
                            />
                        </GridCell>
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Name") + "*"}
                                inputStyle={{ margin: "0" }}
                                withLabel={false}
                                error={state.errors.ExportName}
                                errorMessage={Localization.get("ExportName.ErrorMessage")}
                                value={state.exportRequest.ExportName}
                                onChange={this.onChange.bind(this, "ExportName")}
                                style={{ width: "100%" }}
                            />
                        </GridCell>
                    </div>
                    <div className="right-column">
                        <GridCell>
                            <MultiLineInputWithError
                                inputStyle={{ "minHeight": 110 }}
                                style={{ "width": "100%" }}
                                label={Localization.get("Description") + "*"}
                                value={state.exportRequest.ExportDescription}
                                error={state.errors.ExportDescription}
                                errorMessage={Localization.get("ExportDescription.ErrorMessage")}
                                onChange={this.onChange.bind(this, "ExportDescription")}
                            />
                        </GridCell>
                    </div>
                    <div className="seperator">
                        <hr />
                    </div>
                    <GridSystem>
                        <div className="export-switches">
                            <div className="sectionTitle">{Localization.get("IncludeInExport")}</div>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Content")}
                                />
                                <Switch
                                    value={state.exportRequest.IncludeContent}
                                    onChange={this.onChange.bind(this, "IncludeContent")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Assets")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Assets")}
                                    onChange={this.onChange.bind(this, "Assets")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Users")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Users")}
                                    onChange={this.onChange.bind(this, "Users")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Roles")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Roles")}
                                    onChange={this.onChange.bind(this, "Roles")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Vocabularies")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Vocabularies")}
                                    onChange={this.onChange.bind(this, "Vocabularies")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("PageTemplates")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("PageTemplates")}
                                    onChange={this.onChange.bind(this, "PageTemplates")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ProfileProperties")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Profile_Properties")}
                                    onChange={this.onChange.bind(this, "Profile_Properties")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Permissions")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.includes("Permissions")}
                                    onChange={this.onChange.bind(this, "Permissions")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <div style={{ "paddingBottom": 20, "paddingTop": 10 }}><hr /></div>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("DeletionsInExport")}
                                />
                                <Switch
                                    value={state.exportRequest.IncludeDeletions}
                                    onChange={this.onChange.bind(this, "IncludeDeletions")}
                                />
                            </InputGroup>
                        </div>
                        <div className="export-pages">
                            <div className="sectionTitle">{Localization.get("PagesInExport")}</div>
                            <PagePicker
                                className="export-page-picker"
                                serviceFramework={utilities.utilities && utilities.utilities.sf}
                                PortalTabsParameters={PortalTabsParameters}
                                scrollAreaStyle={scrollAreaStyle}
                                OnSelect={this.updatePagesToExport.bind(this)}
                                allSelected={true}
                                IsMultiSelect={true}
                                IsInDropDown={false}
                                ShowCount={false}
                                Reload={this.state.reloadPages}
                                ShowIcon={false}
                            />
                            <div className="seperator2">
                                <hr />
                            </div>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ExportMode")}
                                />
                                <RadioButtons
                                    onChange={this.onChange.bind(this, "ExportMode")}
                                    options={this.getExportModeOptions()}
                                    buttonGroup="exportMode"
                                    value={state.exportRequest.ExportMode} />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("LastExport")}
                                />
                                <div className="lastExport">{props.lastExportDate}</div>
                            </InputGroup>
                        </div>
                    </GridSystem>
                    <GridCell className="action-buttons">
                        <Button type="secondary" onClick={this.cancelExport.bind(this)}>{Localization.get("Cancel")}</Button>
                        <Button type="primary" onClick={this.onExportPortal.bind(this)}>{Localization.get("BeginExport")}</Button>
                    </GridCell>
                </GridCell>
            </div>
        );
    }
}

ExportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    exportLogs: PropTypes.array,
    viewingLog: PropTypes.bool,
    portalId: PropTypes.number.isRequired,
    portalName: PropTypes.string.isRequired,
    exportJobId: PropTypes.number,
    lastExportDate: PropTypes.string
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.exportWizardStep,
        exportLogs: state.importExport.exportLogs,
        viewingLog: state.importExport.viewingLog,
        exportJobId: state.importExport.exportJobId,
        lastExportDate: state.importExport.lastExportDate
    };
}

export default connect(mapStateToProps)(ExportModal);