import React, { PropTypes } from "react";
import OverwriteContainer from "../containers/ProgressBarOverwriteContainer";

const FileProgress = ({fileName, progress, imgSrc, fileUrl, uploading, error, stopped, message, alreadyExists}) => (
    <div className={"uploading-container" + (error ? " rm-error" : "")  + (uploading ? " rm-uploading" : "") + (stopped ? " rm-stopped" : "")}>
        <a className="file-upload-thumbnail" href={fileUrl} target="_blank">
            <img src={imgSrc} />
        </a>

        <div className="file-name-container">
            <span className="file-name" title={fileName}>{fileName}</span>
            {alreadyExists && <OverwriteContainer fileName={fileName} />}
            {message && <span className="message">{message}</span>}
        </div>
        <div className="progress-bar-container">
            <div className="progress-bar" style={{width: progress + "%"}} />
        </div>
    </div>
);

FileProgress.propTypes = {
    fileName: PropTypes.string,
    progress: PropTypes.number,
    imgSrc: PropTypes.string,
    fileUrl: PropTypes.string,
    uploading: PropTypes.bool,
    error: PropTypes.bool,
    stopped: PropTypes.bool,
    message: PropTypes.string,
    alreadyExists: PropTypes.bool
};

export default FileProgress;