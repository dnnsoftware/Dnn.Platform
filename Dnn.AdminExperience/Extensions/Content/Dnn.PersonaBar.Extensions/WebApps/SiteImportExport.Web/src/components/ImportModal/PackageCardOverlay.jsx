import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization";

class PackageCardOverlay extends Component {
    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        return (
            <div className="package-card-overlay" onClick={props.selectPackage}>
                <div className="icon-container">
                    <div>{props.isSelected ? Localization.get("ClicktoDeselect") : Localization.get("ClicktoSelect")}</div>
                </div>                
            </div>
        );
    }
}

PackageCardOverlay.propTypes = {
    packageName: PropTypes.string,
    packageDescription: PropTypes.string,
    selectPackage: PropTypes.func,
    isSelected: PropTypes.bool
};

export default PackageCardOverlay;