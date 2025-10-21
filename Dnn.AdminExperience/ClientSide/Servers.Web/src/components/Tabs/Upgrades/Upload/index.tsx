import React, { useState, useEffect, useCallback } from "react";
import UploadBar from "./UploadBar";
import Localization from "localization";
import { LocalUpgradeInfo } from "../../../../models/LocalUpgradeInfo";
import "./style.less";

interface ErrorData {
  responseJSON: {
    message: string;
  };
}

interface FileUploadProps {
  cancelInstall: () => void;
  clearUploadedPackage: () => void;
  uploadPackage: (
    file: File,
    onSuccess: (data: any) => void,
    onError: (error: ErrorData) => void
  ) => void;
  uploadComplete: () => void;
}

const FileUpload: React.FC<FileUploadProps> = (props) => {
  const [text, setText] = useState(
    Localization.get("UploadPackage_FileUploadDefault")
  );
  const [draggedOver, setDraggedOver] = useState(false);
  const [fileName, setFileName] = useState("");
  const [uploading, setUploading] = useState(false);
  const [uploadComplete, setUploadComplete] = useState(false);
  const [errorText, setErrorText] = useState("");
  const [errorInPackage, setErrorInPackage] = useState(false);

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
    setText(Localization.get("UploadPackage_FileUploadDefault"));
  }, []);

  const handleError = useCallback((error: ErrorData) => {
    if (error && error.responseJSON && error.responseJSON.message) {
      setUploading(true);
      setErrorText(error.responseJSON.message);
      setErrorInPackage(true);
    } else {
      const errorTextValue = Localization.get(
        "UploadPackage_UploadFailedUnknown"
      );
      setUploading(true);
      setErrorText(errorTextValue);
      setErrorInPackage(true);
    }
  }, []);

  const uploadCompleteCallback = useCallback((data: LocalUpgradeInfo) => {
    setUploading(false);
    setUploadComplete(true);
    setTimeout(() => {
      props.uploadComplete();
    }, 1000);
  }, []);

  const postFile = useCallback(
    (file: File) => {
      if (props.uploadPackage) {
        props.uploadPackage(file, uploadCompleteCallback, handleError);
      }
      setUploading(true);
      setUploadComplete(false);
    },
    [props.uploadPackage, uploadCompleteCallback, handleError]
  );

  const uploadFile = useCallback(
    (file: File) => {
      setFileName(file.name);
      setErrorInPackage(false);
      postFile(file);
    },
    [postFile]
  );

  const onFileUpload = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      if (e.target.files && e.target.files[0]) {
        uploadFile(e.target.files[0]);
      }
    },
    [uploadFile]
  );

  const onDragOver = useCallback(() => {
    setDraggedOver(true);
    setText(Localization.get("UploadPackage_FileUploadDragOver"));
  }, []);

  const onDragLeave = useCallback(() => {
    setDraggedOver(false);
    setText(Localization.get("UploadPackage_FileUploadDefault"));
  }, []);

  const onDrop = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      if (e.dataTransfer.files && e.dataTransfer.files[0]) {
        uploadFile(e.dataTransfer.files[0]);
      }
      onDragLeave();
    },
    [uploadFile, onDragLeave]
  );

  const svg = require(`!raw-loader!./img/upload.svg`).default;

  const buttonsStyle = { width: 67 };
  const className = "overlay" + (draggedOver ? " hover" : "");

  return (
    <div className={"dnn-package-upload" + (uploading ? " uploading" : "")}>
      {(!uploading || uploadComplete) && (
        <div className="dropzone-container">
          <div
            id="dropzoneId"
            className={className}
            onDragOver={onDragOver}
            onDragLeave={onDragLeave}
            onDrop={onDrop}
          >
            <div className="buttons" style={buttonsStyle}>
              <div
                className="button upload"
                onMouseEnter={() =>
                  onMouseEnter(Localization.get("UploadPackage_UploadAFile"))
                }
                onMouseLeave={onMouseLeave}
              >
                <div dangerouslySetInnerHTML={{ __html: svg }} />
                <input type="file" onChange={onFileUpload} aria-label="File" />
              </div>
            </div>
            <span>{text}</span>
          </div>
        </div>
      )}
      {uploading && (
        <UploadBar
          uploadComplete={uploadComplete}
          errorText={errorText}
          errorInPackage={errorInPackage}
          fileName={fileName}
          onTryAgain={() => setUploading(false)}
          tryAgainText={Localization.get("TryAgain")}
          uploadCompleteText={Localization.get("UploadPackage_UploadComplete")}
          uploadingText={Localization.get("UploadPackage_Uploading")}
        />
      )}
    </div>
  );
};

export default FileUpload;
