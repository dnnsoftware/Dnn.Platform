import React, {Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import Label from "dnn-label";
import InfoBlock from "../../common/InfoBlock";
import BackupGrid from "./BackupGrid";
import Localization from "../../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import DatabaseTabActions from "../../../actions/databaseTab";
import utils from "../../../utils";

import "../style.less";

const defaultPlaceHolder = "...";

class Database extends Component {
    componentDidMount() {
        this.props.onRetrieveDatabaseServerInfo();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.utilities.notifyError(newProps.errorMessage);
        }
    }    

    render() {
        const {props} = this;

        return <div>
            <div className="dnn-servers-info-panel">
                <GridSystem>
                    <div>
                        <InfoBlock label={Localization.get("DbInfo_ProductVersion")}
                            tooltip={Localization.get("DbInfo_ProductVersion.Help")}
                            text={props.databaseInfo.productVersion || defaultPlaceHolder} />

                        <InfoBlock label={Localization.get("DbInfo_ServicePack")}
                            tooltip={Localization.get("DbInfo_ServicePack.Help")}
                            text={props.databaseInfo.servicePack || defaultPlaceHolder} />

                        <InfoBlock label={Localization.get("DbInfo_ProductEdition")}
                            tooltip={Localization.get("DbInfo_ProductEdition.Help")}
                            text={props.databaseInfo.productEdition || defaultPlaceHolder} />
                    </div>
                    <div>
                        <InfoBlock label={Localization.get("DbInfo_SoftwarePlatform")}
                            tooltip={Localization.get("DbInfo_SoftwarePlatform.Help")}
                            text={props.databaseInfo.softwarePlatform || defaultPlaceHolder} />
                    </div>
                </GridSystem>
            </div>
            <div className="dnn-servers-grid-panel">
                <Label className="header-title" label={Localization.get("plBackups")} />
                <BackupGrid backups={props.databaseInfo.backups} />
            </div>
        </div>;
    }    
}

Database.propTypes = {   
    databaseInfo: PropTypes.object.isRequired,
    errorMessage: PropTypes.string,
    onRetrieveDatabaseServerInfo: PropTypes.func.isRequired
};

function mapStateToProps(state) {    
    return {
        databaseInfo: state.databaseTab.databaseServerInfo,
        errorMessage: state.databaseTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRetrieveDatabaseServerInfo: DatabaseTabActions.loadDatabaseServerInfo     
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Database);