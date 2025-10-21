import React, { useState, useEffect, useRef } from "react";

/* eslint-disable no-undef */
const upload = require("!raw-loader!./img/upload.svg").default;
const checkmark = require("!raw-loader!./img/checkmark.svg").default;
const errorIcon = require("!raw-loader!./img/x.svg").default;
/* eslint-enable no-undef */

interface UploadBarProps {
    errorText?: string;
    fileName: string;
    uploadComplete: boolean;
    uploadCompleteText: string;
    uploadingText: string;
    errorInPackage: boolean;
    onTryAgain: () => void;
    tryAgainText: string;
}

const UploadBar: React.FC<UploadBarProps> = ({
    errorText,
    fileName,
    uploadComplete,
    uploadCompleteText,
    uploadingText,
    errorInPackage = false,
    onTryAgain,
    tryAgainText
}) => {
    const [percent, setPercent] = useState(0);
    const timeoutRef = useRef(20);
    const deltaRef = useRef(1.00);
    const setTimeoutRef = useRef<NodeJS.Timeout | null>(null);
    const isMountedRef = useRef(true);

    const increase = () => {
        setPercent((prevPercent) => {
            let newPercent = prevPercent + 1;
            timeoutRef.current *= deltaRef.current;
            deltaRef.current *= 1.00;

            if (newPercent < 95) {
                setTimeoutRef.current = setTimeout(increase, timeoutRef.current);
            }

            return newPercent <= 100 ? newPercent : prevPercent;
        });
    };

    useEffect(() => {
        setTimeout(increase, 100);
        isMountedRef.current = true;

        return () => {
            isMountedRef.current = false;
            if (setTimeoutRef.current) {
                clearTimeout(setTimeoutRef.current);
            }
        };
    }, []);

    useEffect(() => {
        if (uploadComplete) {
            if (setTimeoutRef.current) {
                clearTimeout(setTimeoutRef.current);
            }
            if (isMountedRef.current) {
                setPercent(100);
            }
        }
    }, [uploadComplete]);

    const displayPercent = errorText ? 0 : percent;
    const text = errorText ? errorText : (uploadComplete ? uploadCompleteText : uploadingText);
    const svg = errorText ? errorIcon : (uploadComplete ? checkmark : upload);
    const className = "file-upload-container dnn-upload-bar" +
        (uploadComplete ? " complete" : "") +
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