import React, { Component } from "react";
import PropTypes from "prop-types";
import UploadBar from "./UploadBar";
import AlreadyInstalled from "./AlreadyInstalled";
import Localization from "localization";
import LogDisplay from "./LogDisplay";
import RadioButtons from "dnn-radio-buttons";
import "./style.less";

export default class FileUpload extends Component {
    constructor(props) {
        super(props);

        this.state = {
            text: Localization.get("InstallExtension_FileUploadDefault"),


            draggedOver: false,
            isDragging: false,

            fileName: "",

            uploading: false,
            uploadComplete: false,

            errorText: "",
            errorInPackage: false,

            noManifest: false
        };
    }

    componentDidMount() {
        window.addEventListener("dragover", this.prevent);
        window.addEventListener("drop", this.prevent);
    }

    prevent(e) {
        e.preventDefault();
    }

    componentWillUnmount() {
        window.removeEventListener("dragover", this.prevent);
        window.removeEventListener("drop", this.prevent);
    }

    onMouseEnter(text) {
        this.setState({ text });
    }

    onMouseLeave() {
        this.setState({ text: Localization.get("InstallExtension_FileUploadDefault") });
    }


    onFileUpload(e) {
        this.uploadFile(e.target.files[0]);
    }

    getErrorCount(_package) {
        let count = 0;
        _package.forEach(key => {
            if (key.Type === "Failure") {
                count++;
            }
        });
        return count;
    }

    handleError(error) {
        if (error && error.logs && !error.noManifest) {
            const errorCount = this.getErrorCount(error.logs);
            const errorText = error && typeof error === "string" ? error : Localization.get("InstallExtension_UploadFailed") + errorCount + " " + Localization.get("Errors");

            this.setState({ uploading: true, errorText, errorInPackage: true, noManifest: false }, () => {
            });
        } else {
            const errorText = Localization.get("InstallExtension_UploadFailedUnknown");
            this.setState({ uploading: true, errorText, errorInPackage: true, noManifest: false });
        }
    }

    uploadFile(file) {
        this.setState({ fileName: file.name, errorInPackage: false });
        this.postFile(file);
    }

    postFile(file) {
        const { props } = this;
        props.parsePackage(file, this.uploadComplete.bind(this), this.handleError.bind(this));

        this.setState({ uploading: true, uploadComplete: false });
    }

    cancelRepair() {
        this.props.clearParsedInstallationPackage();
        this.setState({
            uploading: false,
            errorInPackage: false,
            errorText: "",
            noManifest: false
        });
    }

    repairInstall() {
        const { props } = this;
        props.repairInstall();
    }

    uploadComplete(data) {
        if (data.noManifest && !this.props.alreadyInstalled) {
            this.setState({
                uploading: false,
                noManifest: true
            });
        } else {
            setTimeout(() => {
                this.setState({ uploadComplete: true }, () => {
                    if (data.alreadyInstalled) {
                        this.setState({
                            uploading: false,
                            noManifest: false
                        });
                    }
                });
            }, 1000);
        }
    }

    onViewLog() {
        this.props.toggleViewLog(true);
    }

    onDragOver() {
        this.setState({ draggedOver: true, text: Localization.get("InstallExtension_FileUploadDragOver") });
    }

    onDragLeave() {
        this.setState({ draggedOver: false, text: Localization.get("InstallExtension_FileUploadDefault") });
    }

    onDrop(e) {
        e.preventDefault();
        this.uploadFile(e.dataTransfer.files[0]);
        this.onDragLeave();
    }
    render() {
        /* eslint-disable quotes */
        const svg = require(`!raw-loader!./img/upload.svg`);

        const buttonsStyle = { width: 67 };
        let className = "overlay" + (this.state.draggedOver ? " hover" : "");

        /* eslint-disable react/no-danger */
        return <div className={"dnn-package-upload" + (this.state.uploading ? " uploading" : "") + (this.props.alreadyInstalled ? " already-installed" : "") + (this.props.viewingLog ? " viewing-log" : "")}>

            {((!this.state.uploading || this.state.uploadComplete) && !this.props.viewingLog) && <div className="dropzone-container">
                <div
                    id="dropzoneId"
                    className={className}
                    onDragOver={this.onDragOver.bind(this)}
                    onDragLeave={this.onDragLeave.bind(this)}
                    onDrop={this.onDrop.bind(this)}>
                    <div className="buttons" style={buttonsStyle}>
                        <div
                            className="button upload"
                            onMouseEnter={this.onMouseEnter.bind(this, Localization.get("InstallExtension_UploadAFile"))}
                            onMouseLeave={this.onMouseLeave.bind(this)}>
                            <div dangerouslySetInnerHTML={{ __html: svg }} />
                            <input type="file" onChange={this.onFileUpload.bind(this)} aria-label="File" />
                        </div>
                    </div>
                    <span>{this.state.text}</span>
                </div>
            </div>
            }
            {this.props.viewingLog &&
                <LogDisplay logs={this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.logs} />}
            {(this.state.uploading && !this.props.viewingLog) &&
                <UploadBar
                    uploadComplete={this.state.uploadComplete}
                    errorText={this.state.errorText}
                    errorInPackage={this.state.errorInPackage}
                    fileName={this.state.fileName}
                    onViewLog={this.onViewLog.bind(this)}
                    onTryAgain={this.cancelRepair.bind(this)}
                    viewLogText={Localization.get("ViewErrorLog")}
                    tryAgainText={Localization.get("TryAgain")}
                    uploadCompleteText={Localization.get("InstallExtension_UploadComplete")}
                    uploadingText={Localization.get("InstallExtension_Uploading")} />
            }
            {
                this.state.noManifest &&
                <div className="no-valid-manifest" style={{ width: "calc(100% + 26px)", height: "calc(100% + 26px)" }}>
                    <p>{Localization.get("InvalidDNNManifest")}</p>
                    <RadioButtons options={[
                        {
                            label: Localization.get("CatalogSkin"),
                            value: "Skin"
                        }, {
                            label: Localization.get("Container"),
                            value: "Container"
                        }
                    ]}
                    buttonGroup="selectedLegacyType"
                    onChange={this.props.onSelectLegacyType.bind(this)}
                    value={this.props.selectedLegacyType} />
                </div>
            }
            {this.props.alreadyInstalled &&
                <AlreadyInstalled
                    fileName={this.state.fileName}
                    cancelRepair={this.cancelRepair.bind(this)}
                    repairInstall={this.repairInstall.bind(this)} />
            }
        </div>;
    }
}


FileUpload.propTypes = {
    cancelInstall: PropTypes.func,
    parsedInstallationPackage: PropTypes.object,
    viewingLog: PropTypes.bool,
    toggleViewLog: PropTypes.func,
    clearParsedInstallationPackage: PropTypes.func,
    onSelectLegacyType: PropTypes.func,
    selectedLegacyType: PropTypes.string,
    alreadyInstalled: PropTypes.bool,
    //---OPTIONAL PROPS---
    buttons: PropTypes.array
};
