import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import PackageCard from "./PackageCard";
import Tooltip from "dnn-tooltip";
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
            return <div className="package-cards">
                {props.importPackages.map((pkg) => {
                    return <div className="package-card-wrapper">
                        <PackageCard selectedPackage={pkg}
                            className={(props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) ? "package-card selected" : "package-card"}>
                            {(!props.selectedPackage || props.selectedPackage.PackageId !== pkg.PackageId) &&
                                <PackageCardOverlay selectPackage={this.onSelect.bind(this, pkg)} packageName={pkg.Name} packageDescription={pkg.Description} />
                            }
                            {props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId &&
                                <div className="checkmark" dangerouslySetInnerHTML={{ __html: CheckMarkIcon }}></div>
                            }
                            <Tooltip
                                onClick={this.onSelect.bind(this, pkg)}
                                messages={[pkg.Description]}
                                tooltipPlace="bottom"
                            />
                        </PackageCard>
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