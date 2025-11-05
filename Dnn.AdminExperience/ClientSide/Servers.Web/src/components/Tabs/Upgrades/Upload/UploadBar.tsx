import React from "react";

/* eslint-disable no-undef */
const upload = require("!raw-loader!./img/upload.svg").default;
const checkmark = require("!raw-loader!./img/checkmark.svg").default;
const errorIcon = require("!raw-loader!./img/x.svg").default;
/* eslint-enable no-undef */

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
    const svg = errorText ? errorIcon : (percent === 100 ? checkmark : upload);
    const className = "file-upload-container dnn-upload-bar" +
        (percent === 100 ? " complete" : "") +
        (errorText ? " upload-error" : "");

    return (
        <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{fileName || "myImage.jpg"}</div>
                <div className="upload-icon" dangerouslySetInnerHTML={{ __html: svg }} />
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