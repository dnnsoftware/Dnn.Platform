import React, { useState, useEffect, useCallback } from "react";
import UploadBar from "./UploadBar";
import Localization from "localization";
import { RadioButtons } from "@dnnsoftware/dnn-react-common";
import "./style.less";

interface Log {
    Type: string;
    Description: string;
}

interface UploadCompleteData {
    noManifest?: boolean;
    alreadyInstalled?: boolean;
}

interface ErrorData {
    logs?: Log[];
    noManifest?: boolean;
}

interface FileUploadProps {
    cancelInstall: () => void;
    clearUploadedPackage: () => void;
    onSelectLegacyType: (value: string) => void;
    selectedLegacyType: string;
    alreadyInstalled: boolean;
    uploadPackage: (
        file: File,
        onSuccess: (data: UploadCompleteData) => void,
        onError: (error: ErrorData | string) => void
    ) => void;
}

const FileUpload: React.FC<FileUploadProps> = ({
    cancelInstall,
    clearUploadedPackage,
    onSelectLegacyType,
    selectedLegacyType,
    alreadyInstalled = false,
    uploadPackage,
}) => {
    const [text, setText] = useState(Localization.get("InstallExtension_FileUploadDefault"));
    const [draggedOver, setDraggedOver] = useState(false);
    const [fileName, setFileName] = useState("");
    const [uploading, setUploading] = useState(false);
    const [uploadComplete, setUploadComplete] = useState(false);
    const [errorText, setErrorText] = useState("");
    const [errorInPackage, setErrorInPackage] = useState(false);
    const [noManifest, setNoManifest] = useState(false);

    const prevent = useCallback((e: Event) => {
        e.preventDefault();
    }, []);

    useEffect(() => {
        window.addEventListener("dragover", prevent);
        window.addEventListener("drop", prevent);

        return () => {
            window.removeEventListener("dragover", prevent);
            window.removeEventListener("drop", prevent);
        };
    }, [prevent]);

    const onMouseEnter = useCallback((text: string) => {
        setText(text);
    }, []);

    const onMouseLeave = useCallback(() => {
        setText(Localization.get("InstallExtension_FileUploadDefault"));
    }, []);

    const getErrorCount = useCallback((_package: Log[]) => {
        let count = 0;
        _package.forEach(key => {
            if (key.Type === "Failure") {
                count++;
            }
        });
        return count;
    }, []);

    const handleError = useCallback((error: ErrorData | string) => {
        if (error && typeof error !== "string" && error.logs && !error.noManifest) {
            const errorCount = getErrorCount(error.logs);
            const errorTextValue =
                typeof error === "string"
                    ? error
                    : errorCount > 0
                        ? (Localization.get("InstallExtension_UploadFailed") + errorCount + " " + Localization.get("Errors"))
                        : Localization.get("InstallExtension_UploadFailedUnknown");

            setUploading(true);
            setErrorText(errorTextValue);
            setErrorInPackage(true);
            setNoManifest(false);
        } else {
            const errorTextValue = Localization.get("InstallExtension_UploadFailedUnknown");
            setUploading(true);
            setErrorText(errorTextValue);
            setErrorInPackage(true);
            setNoManifest(false);
        }
    }, [getErrorCount]);

    const uploadCompleteCallback = useCallback((data: UploadCompleteData) => {
        if (data.noManifest && !alreadyInstalled) {
            setUploading(false);
            setNoManifest(true);
        } else {
            setTimeout(() => {
                setUploadComplete(true);
                if (data.alreadyInstalled) {
                    setUploading(false);
                    setNoManifest(false);
                }
            }, 1000);
        }
    }, [alreadyInstalled]);

    const postFile = useCallback((file: File) => {
        if (uploadPackage) {
            uploadPackage(file, uploadCompleteCallback, handleError);
        }
        setUploading(true);
        setUploadComplete(false);
    }, [uploadPackage, uploadCompleteCallback, handleError]);

    const uploadFile = useCallback((file: File) => {
        setFileName(file.name);
        setErrorInPackage(false);
        postFile(file);
    }, [postFile]);

    const onFileUpload = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            uploadFile(e.target.files[0]);
        }
    }, [uploadFile]);

    const onDragOver = useCallback(() => {
        setDraggedOver(true);
        setText(Localization.get("InstallExtension_FileUploadDragOver"));
    }, []);

    const onDragLeave = useCallback(() => {
        setDraggedOver(false);
        setText(Localization.get("InstallExtension_FileUploadDefault"));
    }, []);

    const onDrop = useCallback((e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();
        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            uploadFile(e.dataTransfer.files[0]);
        }
        onDragLeave();
    }, [uploadFile, onDragLeave]);

    const handleSelectLegacyType = useCallback((value: string) => {
        if (onSelectLegacyType) {
            onSelectLegacyType(value);
        }
    }, [onSelectLegacyType]);

    /* eslint-disable quotes */
    // eslint-disable-next-line no-undef
    const svg = require(`!raw-loader!./img/upload.svg`).default;

    const buttonsStyle = { width: 67 };
    const className = "overlay" + (draggedOver ? " hover" : "");

    return (
        <div className={"dnn-package-upload" + (uploading ? " uploading" : "") + (alreadyInstalled ? " already-installed" : "")}>
            {((!uploading || uploadComplete)) && (
                <div className="dropzone-container">
                    <div
                        id="dropzoneId"
                        className={className}
                        onDragOver={onDragOver}
                        onDragLeave={onDragLeave}
                        onDrop={onDrop}>
                        <div className="buttons" style={buttonsStyle}>
                            <div
                                className="button upload"
                                onMouseEnter={() => onMouseEnter(Localization.get("InstallExtension_UploadAFile"))}
                                onMouseLeave={onMouseLeave}>
                                <div dangerouslySetInnerHTML={{ __html: svg }} />
                                <input type="file" onChange={onFileUpload} aria-label="File" />
                            </div>
                        </div>
                        <span>{text}</span>
                    </div>
                </div>
            )}
            {(uploading) && (
                <UploadBar
                    uploadComplete={uploadComplete}
                    errorText={errorText}
                    errorInPackage={errorInPackage}
                    fileName={fileName}
                    onTryAgain={() => setUploading(false)}
                    tryAgainText={Localization.get("TryAgain")}
                    uploadCompleteText={Localization.get("InstallExtension_UploadComplete")}
                    uploadingText={Localization.get("InstallExtension_Uploading")} />
            )}
            {noManifest && (
                <div className="no-valid-manifest" style={{ width: "calc(100% + 26px)", height: "calc(100% + 26px)" }}>
                    <p>{Localization.get("InvalidDNNManifest")}</p>
                    <RadioButtons
                        options={[
                            {
                                label: Localization.get("CatalogSkin"),
                                value: "Skin"
                            }, {
                                label: Localization.get("Container"),
                                value: "Container"
                            }
                        ]}
                        buttonGroup="selectedLegacyType"
                        onChange={handleSelectLegacyType}
                        value={selectedLegacyType} />
                </div>
            )}
        </div>
    );
};

export default FileUpload;
