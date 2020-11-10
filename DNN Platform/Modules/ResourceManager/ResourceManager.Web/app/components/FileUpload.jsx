import React from "react";
import DropZone from "../containers/DropZoneContainer";
import Progress from "../containers/ProgressContainer";
import localizeService from "../services/localizeService.js";

const FileUpload = () => (
    <div className="file-upload-container">
        <DropZone className="file-upload-panel">
            <a href="javascript:void(0);" className="upload-file active"></a>
            <span>{localizeService.getString("FileUploadPanelMessage")}</span>
        </DropZone>

        <Progress />  
    </div>
);

export default FileUpload;