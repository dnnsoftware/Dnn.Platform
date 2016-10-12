import React, { Component, PropTypes } from "react";
import UploadBar from "./UploadBar";
import AlreadyInstalled from "./AlreadyInstalled";
import Localization from "localization";
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

            errorText: ""
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

    handleError(error) {
        const errorText = error && typeof error === "string" ? error : Localization.get("InstallExtension_UploadFailed");
        const { props } = this;
        props.cancelInstall(true);
        this.setState({ uploading: true, errorText }, () => {
            setTimeout(() => {
                this.setState({ uploading: false, errorText: "" });
            }, 2000);
        });
    }

    uploadFile(file) {
        let format = file.type;
        this.setState({ fileName: file.name });
        this.postFile(file);
    }

    postFile(file) {
        const { props } = this;
        props.parsePackage(file, this.uploadComplete.bind(this), this.handleError.bind(this));

        this.setState({ uploading: true, uploadComplete: false });
    }

    cancelRepair() {
        this.setState({
            alreadyInstalled: false,
            uploading: false
        });
    }

    repairInstall() {
        const { props } = this;
        props.repairInstall();
    }

    uploadComplete(alreadyInstalled) {
        this.setState({ uploadComplete: true }, () => {
            if (alreadyInstalled) {
                this.setState({
                    alreadyInstalled: true,
                    uploading: false
                });
            }
        });
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
        const svg = require(`!raw!./img/upload.svg`);

        const buttonsStyle = { width: 67 };
        let className = "overlay" + (this.state.draggedOver ? " hover" : "");

        /* eslint-disable react/no-danger */
        return <div className={"dnn-package-upload" + (this.state.uploading ? " uploading" : "") + (this.state.alreadyInstalled ? " already-installed" : "")}>
            <div className="dropzone-container">
                <div
                    id="dropzoneId"
                    className={className}
                    onDragOver={this.onDragOver.bind(this)}
                    onDragLeave={this.onDragLeave.bind(this)}
                    onDrop={this.onDrop.bind(this)}
                    >
                    <div className="buttons" style={buttonsStyle}>
                        <div
                            className="button upload"
                            onMouseEnter={this.onMouseEnter.bind(this, Localization.get("InstallExtension_UploadAFile"))}
                            onMouseLeave={this.onMouseLeave.bind(this)}>
                            <div dangerouslySetInnerHTML={{ __html: svg }} />
                            <input type="file" onChange={this.onFileUpload.bind(this)} />
                        </div>
                    </div>
                    <span>{this.state.text}</span>
                </div>
            </div>
            {this.state.uploading &&
                <UploadBar
                    uploadComplete={this.state.uploadComplete}
                    errorText={this.state.errorText}
                    fileName={this.state.fileName}
                    uploadCompleteText={Localization.get("InstallExtension_UploadComplete")}
                    uploadingText={Localization.get("InstallExtension_Uploading")}
                    />
            }
            {this.state.alreadyInstalled &&
                <AlreadyInstalled
                    fileName={this.state.fileName}
                    cancelRepair={this.cancelRepair.bind(this)}
                    repairInstall={this.repairInstall.bind(this)}
                    />
            }
        </div>;
    }
}


FileUpload.propTypes = {
    cancelInstall: PropTypes.func,


    //---OPTIONAL PROPS---
    buttons: PropTypes.array
};

