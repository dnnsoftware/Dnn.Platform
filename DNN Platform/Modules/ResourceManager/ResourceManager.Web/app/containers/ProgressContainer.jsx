import React, { PropTypes } from "react";
import { connect } from "react-redux";
import FileProgress from "../components/FileProgress";
import localizeService from "../services/localizeService";

function isImage(url) {
    if (!url) {
        return false;
    }

    const imageExtensions = ["JPG", "JPE", "BMP", "GIF", "PNG", "JPEG", "ICO"];
    let urlNoParameters = url.split("?")[0];
    let ext = urlNoParameters.split(".").reverse()[0].toUpperCase();
    for (let i = 0; i < imageExtensions.length; i++) {
        if (imageExtensions[i] === ext) return true;
    }
    return false;
}

class ProgressContainer extends React.Component {
    isUploading(fileProgress) {
        const {completed, alreadyExists, stopped, error} = fileProgress;
        return !completed && !alreadyExists && !stopped && !error;
    }
    getImageUrl(fileProgress) {
        const uploading = this.isUploading(fileProgress);

        if (uploading) {
            return "";
        }

        if (isImage(fileProgress.path)) {
            return fileProgress.path;
        }

        return fileProgress.fileIconUrl;
    }

    getMessage(fileProgress) {
        const {error, stopped, completed} = fileProgress;

        if (error) {
            return fileProgress.error;
        }

        if (stopped) {
            return localizeService.getString("FileUploadStoppedMessage");
        }

        if (completed) {
            return localizeService.getString("FileUploadedMessage");
        }

        return null;
    }
    
    render() {
        const { progress } = this.props;
        let output = [];
        for (let fileName in progress) {
            const fileProgress = progress[fileName];
            const {error, stopped, path, alreadyExists} = fileProgress;
            const uploading = this.isUploading(fileProgress);
            const imageUrl = this.getImageUrl(fileProgress);
            const message = this.getMessage(fileProgress);

            output.push(<FileProgress key={fileName} fileName={fileName} progress={fileProgress.percent} imgSrc={imageUrl} fileUrl={path} uploading={uploading} error={error} stopped={stopped} message={message} alreadyExists={alreadyExists} />);
        }

        return (
            <div className="progresses-container">
                { output }
            </div>
        );
    }
}

ProgressContainer.propTypes = {
    progress: PropTypes.object,
    uploaded: PropTypes.object
};

function mapStateToProps(state) {
    const addAssetPanelState = state.addAssetPanel;

    return {
        progress: addAssetPanelState.progress,
        uploaded: addAssetPanelState.uploaded
    };
}

export default connect(mapStateToProps)(ProgressContainer);