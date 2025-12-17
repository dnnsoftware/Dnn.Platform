import React from "react";
import Upload from "./img/upload.svg";
import Checkmark from "./img/checkmark.svg";
import ErrorIcon from "./img/x.svg";

interface UploadBarProps {
    errorText?: string;
    fileName: string;
    percent: number;
    uploadCompleteText: string;
    uploadingText: string;
    errorInPackage: boolean;
    onTryAgain: () => void;
    tryAgainText: string;
}

const UploadBar: React.FC<UploadBarProps> = ({
    errorText,
    fileName,
    percent,
    uploadCompleteText,
    uploadingText,
    errorInPackage = false,
    onTryAgain,
    tryAgainText
}) => {
    const displayPercent = errorText ? 0 : percent;
    const text = errorText ? errorText : (percent === 100 ? uploadCompleteText : uploadingText);
    const UploadIcon = errorText ? ErrorIcon : (percent === 100 ? Checkmark : Upload);
    const className = "file-upload-container dnn-upload-bar" +
        (percent === 100 ? " complete" : "") +
        (errorText ? " upload-error" : "");

    return (
        <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{fileName || "myImage.jpg"}</div>
                <div className="upload-icon"><UploadIcon /></div>
                <h4>{text}</h4>
                {errorInPackage && (
                    <p className="view-log-or-try-again">
                        <span onClick={onTryAgain}>{tryAgainText}</span>
                    </p>
                )}
                <div className="upload-percent">{displayPercent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: displayPercent + "%" }} />
                </div>
            </div>
        </div>
    );
};

export default UploadBar;