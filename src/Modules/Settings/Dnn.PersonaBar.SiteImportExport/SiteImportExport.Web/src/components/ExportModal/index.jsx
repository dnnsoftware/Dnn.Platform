import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
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

class ExportModal extends Component {
    constructor() {
        super();
        this.state = {
            wizardStep: 0,
            exportRequest: {
                PortalId: -1,
                ExportName: "",
                ExportDescription: "",
                ItemsToExport: [],
                Pages: []
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
            if (!props.portals || props.portals.length === 0) {
                props.dispatch(ImportExportActions.getPortals());
            }
        });
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

    }

    getPortalOptions() {
        const { props } = this;
        let options = [];
        if (props.portals) {
            options = props.portals.map((item) => {
                return {
                    label: item.PortalName,
                    value: item.PortalID
                };
            });
        }
        return options;
    }

    onChange(key, event) {
        let { state, props } = this;
        let exportRequest = Object.assign({}, state.exportRequest);

        if (key === "PortalId") {
            exportRequest[key] = event.value;
        }
        else if (key === "ExportName" || key === "ExportDescription") {
            exportRequest[key] = event;
        }
        else {
            exportRequest[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            exportRequest: exportRequest
        });
    }

    updatePagesToExport(selectedPages) {
        let { exportRequest } = this.state;
        exportRequest.pages = selectedPages;
        this.setState({ exportRequest });
    }

    render() {
        const { state, props } = this;
        const { wizardStep } = props;
        const PortalTabsParameters = {
            portalId: props.portalId,
            cultureCode: "en-us",
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
                                label={Localization.get("TemplateFile")}
                                inputStyle={{ margin: "0" }}
                                withLabel={false}
                                error={false}
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
                                label={Localization.get("Description")}
                                value={state.exportRequest.ExportDescription}
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
                                    value={state.exportRequest.ItemsToExport.Content}
                                    onChange={this.onChange.bind(this, "Content")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Assets")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.Assets}
                                    onChange={this.onChange.bind(this, "Assets")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Users")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.Users}
                                    onChange={this.onChange.bind(this, "Users")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Roles")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.Roles}
                                    onChange={this.onChange.bind(this, "Roles")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Vocabularies")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.Vocabularies}
                                    onChange={this.onChange.bind(this, "Vocabularies")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ProfileProperties")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.ProfileProperties}
                                    onChange={this.onChange.bind(this, "ProfileProperties")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Permissions")}
                                />
                                <Switch
                                    value={state.exportRequest.ItemsToExport.Permissions}
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
    portalId: PropTypes.number,
    portals: PropTypes.array
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.exportWizardStep,
        exportLogs: state.importExport.exportLogs,
        viewingLog: state.importExport.viewingLog,
        portals: state.importExport.portals,
        portalId: state.importExport.portalId
    };
}

export default connect(mapStateToProps)(ExportModal);