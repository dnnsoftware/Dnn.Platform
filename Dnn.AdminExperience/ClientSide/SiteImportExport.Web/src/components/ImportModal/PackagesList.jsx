import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Localization from "localization";
import PackageCard from "./PackageCard";
import PackageCardOverlay from "./PackageCardOverlay";

import { SvgIcons } from "@dnnsoftware/dnn-react-common";

class PackagesList extends Component {
    onSelect(pkg) {
        this.props.selectPackage(pkg);
    }

    renderTooltipMessage(description) {
        if (description) {
            return (
                "<div><div style='text-transform: uppercase;font-weight: 700;padding: 20px 20px 15px 20px;color: #000'>" +
                Localization.get("PackageDescription") +
                "</div><div style='color: #4b4e4f;padding: 0 20px 20px 20px;'>" +
                description +
                "</div></div>"
            );
        }
        else return;
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        if (props.importPackages && props.importPackages.length > 0) {
            return <div className="package-cards">
                {props.importPackages.map((pkg, i) => {
                    return <div className="package-card-wrapper" key={i}>
                        <PackageCard selectedPackage={pkg}
                            className={(props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) ? "package-card selected" : "package-card"}>
                            <PackageCardOverlay
                                selectPackage={this.onSelect.bind(this, pkg)}
                                packageName={pkg.Name}
                                packageDescription={pkg.Description}
                                isSelected={props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId} />
                            {props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId &&
                                <div className="checkmark" dangerouslySetInnerHTML={{ __html: SvgIcons.CheckMarkIcon }}></div>
                            }                            
                        </PackageCard>
                        {
                            pkg.Description &&
                            <div 
                                className="package-card-tooltip"
                                dangerouslySetInnerHTML={{ __html : this.renderTooltipMessage(pkg.Description)}}
                                onClick={this.onSelect.bind(this, pkg)}
                            />
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