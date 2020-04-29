import React, { PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import addAssetPanelActions from "../actions/addAssetPanelActions";
import localizeService from "../services/localizeService.js";

class ProgressBarOverwriteContainer extends React.Component {
    render() {
        const {fileName, folderPanelState, overwriteFile, stopUpload, trackProgress, breadcrumbs} = this.props;

        function getFolderPath() {
            let path = "";
            for (let i = 1; i < breadcrumbs.length; i++) {
                path += breadcrumbs[i].folderName + "/";
            }

            return path;
        }

        function overwriteClickHandler() {
            overwriteFile(fileName, getFolderPath(), folderPanelState, trackProgress, true);
        }

        function abortClickHandler() {
            stopUpload(fileName);
        }

        return (
            <span>
                <label>{localizeService.getString("FileAlreadyExistsMessage")}</label>
                <a onClick={overwriteClickHandler}>{localizeService.getString("Replace")}</a>
                <a onClick={abortClickHandler}>{localizeService.getString("Keep")}</a>
            </span>
        );
    }
}

ProgressBarOverwriteContainer.propTypes = {
    fileName: PropTypes.string,
    breadcrumbs: PropTypes.array,
    file: PropTypes.object,
    folderPanelState: PropTypes.object,
    overwriteFile: PropTypes.func,
    stopUpload: PropTypes.func,
    trackProgress: PropTypes.func,
    progress: PropTypes.object
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const breadcrumbsState = state.breadcrumbs;

    return {
        folderPanelState,
        breadcrumbs: breadcrumbsState.breadcrumbs
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            overwriteFile: addAssetPanelActions.overwriteFile.bind(addAssetPanelActions),
            stopUpload: addAssetPanelActions.stopUpload,
            trackProgress: addAssetPanelActions.trackProgress
        }, dispatch)
    };
}


export default connect(mapStateToProps, mapDispatchToProps)(ProgressBarOverwriteContainer);