import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import Label from "dnn-label";
import GridCell from "dnn-grid-cell";
import PackageCardOverlay from "./PackageCardOverlay";
import {
    CheckMarkIcon
} from "dnn-svg-icons";

class PackagesList extends Component {
    onSelect(pkg) {
        this.props.selectPackage(pkg);
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        if (props.importPackages && props.importPackages.length > 0) {
            return <div className="package-card-wrapper">
                {props.importPackages.map((pkg) => {
                    return <div className={(props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) ? "package-card selected" : "package-card"}>
                        <div className="card-grid">
                            <GridCell columnSize={35} className="card-column1">
                                <div className="package-name">
                                    <TextOverflowWrapper text={pkg.Name} maxWidth={200} />
                                </div>
                                <div className="package-field">
                                    <TextOverflowWrapper text={pkg.ExporTime} maxWidth={200} />
                                </div>
                            </GridCell>
                            <GridCell columnSize={17} className="card-column2">
                                <div className="package-field">
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("FileName")} />
                                </div>
                                <div className="package-field">
                                    <Label
                                        labelType="inline"
                                        label={Localization.get("Website")} />
                                </div>
                            </GridCell>
                            <GridCell columnSize={23} className="card-column3">
                                <div className="package-field">
                                    <TextOverflowWrapper text={pkg.FileName} maxWidth={160} />
                                </div>
                                <div className="package-field">
                                    <TextOverflowWrapper text={pkg.PortalName} maxWidth={160} />
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
                                    <TextOverflowWrapper text={"Differential"} maxWidth={70} />
                                </div>
                                <div className="package-field">
                                    <TextOverflowWrapper text={"1.2 GB"} maxWidth={70} />
                                </div>
                            </GridCell>
                        </div>
                        {(!props.selectedPackage || props.selectedPackage.PackageId !== pkg.PackageId) &&
                            <PackageCardOverlay selectPackage={this.onSelect.bind(this, pkg)} packageName={pkg.Name} packageDescription={pkg.Description} />
                        }
                        {props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId &&
                            <div className="checkmark" dangerouslySetInnerHTML={{ __html: CheckMarkIcon }}></div>
                        }
                    </div>;
                })}
            </div>;
        }
        else return <div className="noPackages">{Localization.get("NoPackages")}</div>;
    }
}

PackagesList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    importPackages: PropTypes.array,
    selectedPackage: PropTypes.object,
    selectPackage: PropTypes.func
};

function mapStateToProps(state) {
    return {
        importPackages: state.importExport.importPackages,
        selectedPackage: state.importExport.selectedPackage
    };
}

export default connect(mapStateToProps)(PackagesList);