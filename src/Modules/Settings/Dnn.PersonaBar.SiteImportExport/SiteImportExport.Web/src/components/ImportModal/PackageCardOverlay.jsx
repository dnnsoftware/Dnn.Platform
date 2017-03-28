import React, { PropTypes, Component } from "react";
import Tooltip from "dnn-tooltip";
import Localization from "localization";
import {
    importExport as ImportExportActions
} from "../../actions";

class PackageCardOverlay extends Component {
    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        return (
            <div className="package-card-overlay" onClick={props.selectPackage}>
                <Tooltip type="info"
                    messages={[props.packageDescription]}
                    tooltipPlace="bottom" />
                <div className="icon-container">
                    <div>{Localization.get("ClicktoSelect")}</div>
                </div>
            </div >
        );
    }
}

PackageCardOverlay.propTypes = {
    packageName: PropTypes.string,
    packageDescription: PropTypes.string,
    selectPackage: PropTypes.func
};

export default PackageCardOverlay;