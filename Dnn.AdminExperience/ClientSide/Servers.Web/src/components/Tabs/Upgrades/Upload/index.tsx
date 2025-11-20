import React, { useState, useEffect, useCallback } from "react";
import UploadBar from "./UploadBar";
import Localization from "localization";
import "./style.less";
import { ChunkToUpload } from "models/ChunkToUpload";
import upgradeService from "services/upgradeService";

interface ErrorData {
  responseJSON: {
    message: string;
  };
}

interface FileUploadProps {
  cancelInstall: () => void;
  clearUploadedPackage: () => void;
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
  const [percent, setPercent] = useState(0);

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

  const uploadCompleteCallback = useCallback(() => {
    setUploading(false);
    setUploadComplete(true);
    setPercent(100);
    props.uploadComplete();
  }, []);

  const uploadFile = useCallback(
    async (file: File) => {
      setFileName(file.name);
      setErrorInPackage(false);
      setUploading(true);
      setUploadComplete(false);
      setPercent(0);

      const newFileId = Math.random().toString(36).substring(2, 15);
      const chunkSize = 1024 * 1024; // size of each chunk (1MB)
      let start = 0;

      // Upload chunks sequentially
      while (start < file.size) {
        const chunk: ChunkToUpload = {
          chunk: file.slice(start, start + chunkSize),
          start: start,
          totalSize: file.size,
          fileId: newFileId,
        };

        // Wait for this chunk to complete before uploading the next one
        try {
          await new Promise<void>((resolve, reject) => {
            upgradeService.uploadPackage(
              chunk,
              () => {
                // Only call the complete callback on the last chunk
                setPercent(
                  Math.min(
                    100,
                    Math.floor(((start + chunkSize) / file.size) * 100)
                  )
                );
                if (start + chunkSize >= file.size) {
                  upgradeService
                    .completeUpload(newFileId, file.name)
                    .then(() => {
                      uploadCompleteCallback();
                    })
                    .catch((error) => {
                      handleError(error);
                      reject(error);
                    });
                }
                resolve();
              },
              (error) => {
                handleError(error);
                reject(error);
              }
            );
          });
        } catch (error) {
          break;
        }

        start += chunkSize;
      }
    },
    [uploadCompleteCallback, handleError]
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
          percent={percent}
          errorText={errorText}
          errorInPackage={errorInPackage}
          fileName={fileName}
          onTryAgain={() => {
            setFileName("");
            setErrorText("");
            setErrorInPackage(true);
            setUploading(false);
            setUploadComplete(false);
            setPercent(0);
          }}
          tryAgainText={Localization.get("TryAgain")}
          uploadCompleteText={Localization.get("UploadPackage_UploadComplete")}
          uploadingText={Localization.get("UploadPackage_Uploading")}
        />
      )}
    </div>
  );
};

export default FileUpload;
