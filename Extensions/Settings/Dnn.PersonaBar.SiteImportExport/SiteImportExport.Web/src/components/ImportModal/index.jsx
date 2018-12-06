import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions,
    visiblePanel as VisiblePanelActions
} from "../../actions";
import Localization from "localization";
import ImportSummary from "./ImportSummary";
import PackagesList from "./PackagesList";
import PackageCard from "./PackageCard";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import FiltersBar from "./FiltersBar";
import ProgressBar from "./ProgressBar";
import Pager from "dnn-pager";
import styles from "./style.less";
import util from "utils";

const noDataImg = require("!raw-loader!./img/nodata.svg");

class ImportModal extends Component {
    constructor() {
        super();
        this.state = {
            importRequest: {
                PortalId: -1,
                PackageId: -1,
                CollisionResolution: 1,
                RunNow: true
            },
            pageIndex: 0,
            pageSize: 5,
            filter: "newest",
            keyword: "",
            requestSubmitting: false
        };
    }

    UNSAFE_componentWillMount() {
        const { props, state } = this;
        props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));

        const { importRequest } = state;
        importRequest.PortalId = props.portalId;
        importRequest.PackageId = props.selectedPackage ? props.selectedPackage.PackageId : -1;
        this.setState({
            importRequest
        });
    }

    UNSAFE_componentWillReceiveProps(props) {
        const { state } = this;
        const { importRequest } = state;
        if (importRequest.PortalId === -1 || importRequest.PortalId !== props.portalId) {
            importRequest.PortalId = props.portalId;
            this.setState({
                importRequest
            });
        }
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
        if (props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) {
            props.dispatch(ImportExportActions.selectPackage());
        }
        else {
            props.dispatch(ImportExportActions.selectPackage(pkg, () => {
                let { importRequest } = this.state;
                importRequest.PackageId = pkg.PackageId;
                this.setState({
                    importRequest
                });
            }));
        }
    }

    cancelImport() {
        const { props } = this;
        if (props.wizardStep === 0) {
            this.goToStep(0);
            props.onCancel();
            props.dispatch(ImportExportActions.packageVerified(false));
            props.dispatch(ImportExportActions.importWizardGoToSetp(0));
        }
        else {
            util.utilities.confirm(Localization.get("CancelImportMessage"),
                Localization.get("ConfirmCancel"),
                Localization.get("KeepImport"),
                () => {
                    props.dispatch(ImportExportActions.packageVerified(false));
                    props.dispatch(ImportExportActions.selectPackage());
                    props.dispatch(ImportExportActions.importWizardGoToSetp(0));
                });
        }
    }

    onAnalyze() {
        const { props } = this;
        if (props.selectedPackage) {
            props.dispatch(ImportExportActions.importWizardGoToSetp(1, () => {
                props.dispatch(ImportExportActions.verifyImportPackage(props.selectedPackage.PackageId, () => { }, () => {
                    props.dispatch(ImportExportActions.packageVerified(true));
                }));
            }));
        }
        else {
            util.utilities.notifyError(Localization.get("SelectException"));
        }
    }

    onImport() {
        const { props, state } = this;
        this.setState({
            requestSubmitting: true
        });
        props.dispatch(ImportExportActions.importSite(state.importRequest, (data) => {
            this.setState({
                requestSubmitting: false
            });
            util.utilities.notify(Localization.get("ImportRequestSubmitted"));
            props.dispatch(ImportExportActions.packageVerified(false));
            this.goToStep(0);
            props.dispatch(ImportExportActions.getAllJobs({
                portal: state.importRequest.PortalId,
                pageIndex: 0,
                pageSize: 10
            }, () => {
                props.dispatch(ImportExportActions.jobSelected(data.jobId));
            }));
            props.dispatch(VisiblePanelActions.selectPanel(0));
        }, () => {
            this.setState({
                requestSubmitting: false
            });
            util.utilities.notifyError(Localization.get("ImportRequestSubmit.ErrorMessage"));
        }));
    }

    getSummaryItem(category) {
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category === category.toUpperCase());
        return detail ? detail.TotalItemsString : "-";
    }

    onSwitchChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let { importRequest } = this.state;
        if (key === "CollisionResolution") {
            importRequest.CollisionResolution = value ? 1 : 0;
        }
        else if (key === "RunNow") {
            importRequest.RunNow = value;
        }
        this.setState({
            importRequest
        });
    }

    getNextPage() {
        const { state } = this;
        return {
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize,
            order: state.filter,
            keyword: state.keyword
        };
    }

    onFilterChanged(filter) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            filter: filter.value
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
    }

    onKeywordChanged(keyword) {
        const { props } = this;
        this.setState({
            pageIndex: 0,
            keyword: keyword
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
    }

    onPageChange(currentPage, pageSize) {
        let { state, props } = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
    }

    /* eslint-disable react/no-danger */
    renderPackageVerification() {
        const { props } = this;
        return <div>
            {props.selectedPackage && !props.packageVerified &&
                <div className="package-analyzing">
                    <div className="noDataText">{Localization.get("VerifyPackage")}</div>
                    <div className="noDataImage" dangerouslySetInnerHTML={{ __html: noDataImg }}></div>
                    <ProgressBar className="progressCards" visible={true} loaded={props.importSummary} />
                </div>}
        </div>;
    }

    renderPager() {
        const { props, state } = this;
        return (
            <div className="packagePager">
                {props.importPackages && <Pager
                    showStartEndButtons={true}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalPackages}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture()}
                />}
            </div>
        );
    }

    render() {
        const { props, state } = this;
        return (
            <div className={styles.importModal}>
                <div className="pageTitle">{Localization.get("SelectImportPackage")}</div>
                <div className="packages-wrapper">
                    {props.wizardStep === 0 &&
                        <FiltersBar onFilterChanged={this.onFilterChanged.bind(this)}
                            onKeywordChanged={this.onKeywordChanged.bind(this)}
                        />
                    }
                    <div className="packages">
                        {props.wizardStep === 0 &&
                            <PackagesList selectPackage={this.selectPackage.bind(this)} />
                        }
                        {props.wizardStep === 1 && props.selectedPackage &&
                            <div className="package-card-wrapper">
                                <PackageCard selectedPackage={props.selectedPackage} />
                            </div>
                        }
                        {props.wizardStep === 1 && !props.packageVerified &&
                            this.renderPackageVerification()
                        }
                    </div>
                    {props.wizardStep === 0 &&
                        this.renderPager()
                    }
                    {props.wizardStep === 1 && props.packageVerified &&
                        <ImportSummary
                            collisionResolution={state.importRequest.CollisionResolution}
                            runNow={state.importRequest.RunNow}
                            onSwitchChange={this.onSwitchChange.bind(this)} />
                    }
                </div>
                <GridCell className="action-buttons">
                    <Button type="secondary" onClick={this.cancelImport.bind(this)}>{Localization.get("Cancel")}</Button>
                    {props.wizardStep === 0 &&
                        <Button type="primary"
                            disabled={props.selectedPackage ? false : true}
                            onClick={this.onAnalyze.bind(this)}>
                            {Localization.get("Continue")}
                        </Button>
                    }
                    {props.wizardStep === 1 &&
                        <Button type="primary"
                            disabled={!props.importSummary || state.requestSubmitting}
                            onClick={this.onImport.bind(this)}>
                            {Localization.get("Continue")}
                        </Button>
                    }
                </GridCell>
            </div>
        );
    }
}

ImportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number.isRequired,
    portalName: PropTypes.string.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    importPackages: PropTypes.array,
    selectedPackage: PropTypes.object,
    importSummary: PropTypes.object,
    totalPackages: PropTypes.number,
    packageVerified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        wizardStep: state.importExport.importWizardStep,
        importPackages: state.importExport.importPackages,
        selectedPackage: state.importExport.selectedPackage,
        importSummary: state.importExport.importSummary,
        totalPackages: state.importExport.totalPackages,
        portalId: state.importExport.portalId,
        packageVerified: state.importExport.packageVerified
    };
}

export default connect(mapStateToProps)(ImportModal);