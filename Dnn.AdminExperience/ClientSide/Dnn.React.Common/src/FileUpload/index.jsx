import React, { Component } from "react";
import PropTypes from "prop-types";
import LinkInput from "./LinkInput";
import Browse from "./Browse";
import UploadBar from "./UploadBar";

import "./style.less";

const Buttons = [
    { name: "browse" },
    { name: "upload" },
    { name: "link" }
];

export default class FileUpload extends Component {
    constructor(props) {
        super(props);
        let fileExist = false;
        let selectedFile = null;
        let selectedFolder = null;

        this.fileInput1Ref = React.createRef();
        this.fileInput2Ref = React.createRef();

        this.state = {
            text: props.defaultText,
            showLinkInput: false,
            showFolderPicker: false,

            selectedFile,
            selectedFolder,

            linkPath: "",
            fileUrl: "",

            fileExist,
            draggedOver: false,
            isDragging: false,

            fileName: "",

            uploading: false,
            uploadComplete: false,
            horizontalOrientation: false,

            errorText: ""
        };
        this.compareTimeout = setTimeout(this.compareDimensions.bind(this), 2000);
    }

    updateStateAndReloadImage(file) {
        const selectedFolder = { value: file.folderPath, key: file.folderId };
        const selectedFile = { value: file.fileName, key: file.fileId };
        const fileExist = true;

        this.setState({fileExist, selectedFile, selectedFolder}, () => {
            this.getPreviewUrl(file.fileId);
        });
    }

    componentDidMount() {
        const file = this.props.selectedFile;
        if (file) {
            this.updateStateAndReloadImage(file);
        }
        window.addEventListener("dragover", this.prevent);
        window.addEventListener("drop", this.prevent);
    }

    componentDidUpdate(prevProps) {
        const { props } = this;
        if (!props.selectedFile && props.selectedFile !== prevProps.selectedFile) {
            this.setState({ fileExist: null, selectedFile: null, selectedFolder: null }, () => {});
            return;
        }
        if (props.selectedFile && props.selectedFile !== prevProps.selectedFile && this.state.selectedFile)
        {
            if (props.selectedFile.fileId !== (this.state.selectedFile.fileId || + this.state.selectedFile.key)) {
                const file = props.selectedFile;
                this.updateStateAndReloadImage(file);
            }
        }
        if (props.portalId !== prevProps.portalId) {         
            this.setState({ showFolderPicker: false });
        }
    }

    prevent(e) {
        e.preventDefault();
    }

    componentWillUnmount() {
        window.removeEventListener("dragover", this.prevent);
        window.removeEventListener("drop", this.prevent);

        this._unmounted = true;
        if (this.compareTimeout) {
            clearTimeout(this.compareTimeout);
            this.compareTimeout = null;
        }
    }

    onLink() {
        if (window.dnn !== undefined) {
            window.dnn.stopEscapeFromClosingPB = true;
        }
        this.setState({ showLinkInput: true });
    }

    onBrowse() {
        if (window.dnn !== undefined) {
            window.dnn.stopEscapeFromClosingPB = true;
        }
        this.setState({ showFolderPicker: true });
    }

    onButtonClick(action) {
        switch (action) {
            case "link":
                return this.onLink();
            case "browse":
                return this.onBrowse();
        }
    }

    hideFields() {
        if (window.dnn !== undefined) {
            window.dnn.stopEscapeFromClosingPB = false;
        }
        this.setState({ showLinkInput: false, showFolderPicker: false });
    }

    onMouseEnter(text) {
        this.setState({ text });
    }

    onMouseLeave() {
        this.setState({ text: this.props.defaultText });
    }

    onFileSelect(selectedFolder, selectedFile) {
        this.setState({ selectedFolder, selectedFile }, () => {
            this.getPreviewUrl();
            this.sendResult();
        });
        this.hideFields();
    }


    handleImageError() {
        this.setState({ fileExist: false });
    }

    onFileUpload(e) {
        this.uploadFile(e.target.files[0]);
    }

    handleError(error) {
        const errorText = error && typeof error === "string" ? error : this.props.uploadFailedText;
        this.setState({ uploading: true, errorText }, () => {
            setTimeout(() => {
                this.setState({ uploading: false, errorText: "" });
            }, 2000);
        });
    }

    uploadFile(file) {
        if (!file) {
            return;
        }
        const fileFormats = this.props.fileFormats;
        this.setState({ fileName: file.name });
        if (fileFormats.length > 0) {
            let format = file.type;
            const isAcceptFormat = fileFormats.some(f => format === f);
            if (!isAcceptFormat) {
                return this.handleError(this.props.wrongFormatText);
            }
        }

        this.postFile(file);
    }

    getServiceFramework() {
        let sf = this.props.utils.utilities.sf;
        sf.controller = "FileUpload";
        sf.moduleRoot = "InternalServices";
        return sf;
    }

    uploadFromLink(fileUrl) {
        this.setState({ fileUrl: "" });
        this.uploadFromUrl(fileUrl);
        this.hideFields();
    }

    showPreview(fileUrl) {
        if (this._unmounted) {
            return;
        }

        this.setState({ fileUrl: "" });
        if (typeof fileUrl !== "string") {
            return;
        }
        this.getImageDimensions(fileUrl);
        this.setState({ fileUrl, fileExist: true });
    }

    sendResult() {
        const selectedFile = this.state.selectedFile;
        const selectedFolder = this.state.selectedFolder;
        const fileId = selectedFile ? selectedFile.fileId || +selectedFile.key : null;
        const folderPath = selectedFolder ? selectedFolder.value : null;
        const fileName = selectedFile ? selectedFile.value : null;
        this.props.onSelectFile({
            folderPath,
            fileId,
            fileName
        });
    }

    uploadFromUrl(url) {
        const folder = this.props.folderName && typeof this.props.folderName === "string" ? this.props.folderName : "";
        const sf = this.getServiceFramework();
        sf.post("UploadFromUrl", { url, folder }, this.uploadComplete.bind(this), this.handleError.bind(this));
        this.setState({ uploading: true, uploadComplete: false });
    }

    getPreviewUrl() {
        const fileId = this.state.selectedFile ? this.state.selectedFile.fileId || +this.state.selectedFile.key : "";
        if (!fileId) {
            this.setState({ fileUrl: "", fileExist: false }, this.sendResult.bind(this));
        } else {
            const sf = this.getServiceFramework();
            sf.get("loadimage", { fileId }, this.showPreview.bind(this), this.callback);
        }
    }

    getImageDimensions(src) {
        let tempImage = new Image();
        tempImage.src = src;
        tempImage.onload = this.compareDimensions.bind(this, tempImage);
    }

    compareDimensions(image) {
        if (!image) {
            return;
        }
        const componentDimension = this.node.getBoundingClientRect();

        if (image.height && image.width / image.height > componentDimension.width / componentDimension.height) {
            this.setState({ horizontalOrientation: true });
        } else {
            this.setState({ horizontalOrientation: false });
        }
    }

    postFile(file) {
        const formData = new FormData();
        formData.append("postfile", file);
        const sf = this.getServiceFramework();
        if (this.props.folderName && typeof this.props.folderName === "string") {
            formData.append("folder", this.props.folderName);
        }
        if (this.props.validationCode && typeof this.props.validationCode === "string") {
            formData.append("validationCode", this.props.validationCode);
        }
        sf.postfile(`UploadFromLocal${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}` , formData, this.uploadComplete.bind(this), this.handleError.bind(this));
        this.setState({ uploading: true, uploadComplete: false });

        this.clearFileUploaderValue(this.fileInput1Ref);
        this.clearFileUploaderValue(this.fileInput2Ref);
    }

    clearFileUploaderValue(fileInput) {
        if (fileInput) {
            fileInput.value = "";
        }
    }

    uploadComplete(res) {
        this.setState({ uploadComplete: true }, () => {
            setTimeout(() => {
                this.setState({ uploading: false });
            }, 1000);
        });
        const response = typeof res === "string" ? JSON.parse(res) : res;
        if (!response.path) {
            return;
        }
        const selectedFile = { value: response.fileName, fileId: response.fileId };
        this.setState({ selectedFile }, () => {
            this.getPreviewUrl();
            this.sendResult();
        });
    }

    callback(result) {
    }

    onDragOver() {
        this.setState({ draggedOver: true, text: this.props.onDragOverText });
    }

    onDragLeave() {
        this.setState({ draggedOver: false, text: this.props.defaultText });
    }

    onDrop(e) {
        e.preventDefault();
        this.uploadFile(e.dataTransfer.files[0]);
        this.onDragLeave();
    }

    getImageStyle() {
        const {cropImagePreview} = this.props;
        const {horizontalOrientation} = this.state;
        let style = { width: "100%", height: "auto" };

        if (horizontalOrientation && cropImagePreview) {
            style = { height: "100%", width: "auto" };
        }
        if (!horizontalOrientation && !cropImagePreview) {
            style = { height: "100%", width: "auto" };
        }
        return style;
    }

    onChangeUrl(url) {
        const {props, state} = this;
        this.setState({fileUrl: url});
    }

    render() {
        const {props, state} = this;

        let buttons = Buttons;
        if (props.buttons) {
            buttons = buttons.filter((button) => {
                return props.buttons.some((propButton) => {
                    return button.name === propButton;
                });
            });
        }

        buttons = buttons.map((button) => {
            const svg = require(`!raw-loader!./img/${button.name}.svg`).default;
            const isUpload = button.name === "upload";
            /* eslint-disable react/no-danger */
            const accept = props.fileFormats.join(",");
            return <div
                className={"button " + button.name}
                onMouseEnter={this.onMouseEnter.bind(this, props[button.name + "ButtonText"]) }
                onMouseLeave={this.onMouseLeave.bind(this) }
                onClick={this.onButtonClick.bind(this, button.name) }
                key={button.name}>
                <div dangerouslySetInnerHTML={{ __html: svg }} />
                {isUpload && accept && <input type="file" ref={this.fileInput1Ref} accept={accept} onChange={this.onFileUpload.bind(this) } aria-label="File" />}
                {isUpload && !accept && <input type="file" ref={this.fileInput2Ref} onChange={this.onFileUpload.bind(this) } aria-label="File" />}
            </div>;
        });

        const buttonsStyle = { width: buttons.length * 67 };
        const src = state.fileUrl || "";
        const showImage = src && state.fileExist && !state.showLinkInput && !state.showFolderPicker;
        const className = "overlay" + (src && state.fileExist ? " has-image" : "") + (state.draggedOver ? " hover" : "");

        return <div className="dnn-file-upload" ref={node => this.node = node}>
            <div>
                <div
                    id="dropzoneId"
                    className={className}
                    onDragOver={this.onDragOver.bind(this) }
                    onDragLeave={this.onDragLeave.bind(this) }
                    onDrop={this.onDrop.bind(this) }
                >
                    <div className="buttons" style={buttonsStyle}>
                        {buttons}
                    </div>
                    <span>{state.text}</span>
                </div>

                {state.showLinkInput && <LinkInput
                    linkInputTitleText={props.linkInputTitleText}
                    linkInputPlaceholderText={props.linkInputPlaceholderText}
                    linkInputActionText={props.linkInputActionText}
                    linkPath={state.linkPath}
                    onSave={this.uploadFromLink.bind(this)}
                    onCancel={this.hideFields.bind(this)}
                    onChange={this.onChangeUrl.bind(this)}
                />}
                {state.showFolderPicker && <Browse
                    portalId={props.portalId}
                    browseActionText={props.browseActionText}
                    fileText={props.fileText}
                    folderText={props.folderText}
                    notSpecifiedText={props.notSpecifiedText}
                    searchFoldersPlaceHolderText={props.searchFoldersPlaceHolderText}
                    searchFilesPlaceHolderText={props.searchFilesPlaceHolderText}
                    utils={props.utils}
                    fileFormats={props.fileFormats}
                    selectedFolder={state.selectedFolder}
                    selectedFile={state.selectedFile}
                    onSave={this.onFileSelect.bind(this) }
                    onCancel={this.hideFields.bind(this) }
                />}
                {showImage && <div className="image-container">
                    <img
                        style={this.getImageStyle() }
                        onError={this.handleImageError.bind(this) }
                        src={src} alt={props.imageText}/></div>}
                {state.selectedFile &&
                    <div className="dnn-file-upload-file-name"><span>{state.selectedFile.value}</span></div>}
            </div>
            {state.uploading && <UploadBar
                uploadCompleteText={props.uploadCompleteText}
                uploadingText={props.uploadingText}
                uploadDefaultText={props.uploadDefaultText}
                uploadComplete={state.uploadComplete}
                errorText={state.errorText}
                fileName={state.fileName}
            />}
        </div>;
    }
}


FileUpload.propTypes = {
    //---REQUIRED PROPS---
    utils: PropTypes.object.isRequired,
    onSelectFile: PropTypes.func.isRequired,
    validationCode: PropTypes.string.isRequired,

    //---OPTIONAL PROPS---
    selectedFile: PropTypes.object,
    cropImagePreview: PropTypes.bool,
    buttons: PropTypes.array,
    folderName: PropTypes.string,
    portalId: PropTypes.number,
    fileFormats: PropTypes.array,

    //-- Localization Props---
    browseButtonText: PropTypes.string,
    uploadButtonText: PropTypes.string,
    linkButtonText: PropTypes.string,
    defaultText: PropTypes.string,
    onDragOverText: PropTypes.string,
    uploadFailedText: PropTypes.string,
    wrongFormatText: PropTypes.string,
    imageText: PropTypes.string,
    linkInputTitleText: PropTypes.string,
    linkInputPlaceholderText: PropTypes.string,
    linkInputActionText: PropTypes.string,
    uploadCompleteText: PropTypes.string,
    uploadingText: PropTypes.string,
    uploadDefaultText: PropTypes.string,
    browseActionText: PropTypes.string,
    notSpecifiedText: PropTypes.string,
    searchFilesPlaceHolderText: PropTypes.string,
    searchFoldersPlaceHolderText: PropTypes.string,
    fileText: PropTypes.string,
    folderText: PropTypes.string
};

FileUpload.defaultProps = {
    cropImagePreview: false,
    portalId: -1,
    fileFormats: [],
    browseButtonText: "Browse Filesystem",
    uploadButtonText: "Upload a File",
    linkButtonText: "Enter URL Link",
    defaultText: "Drag and Drop a File or Select an Option",
    onDragOverText: "Drag and Drop a File",
    uploadFailedText: "Upload Failed",
    wrongFormatText: "wrong format",
    imageText: "Image",
    linkInputTitleText: "URL Link",
    linkInputPlaceholderText: "http://example.com/imagename.jpg",
    linkInputActionText: "Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel",
    uploadCompleteText: "Upload Complete",
    uploadingText: "Uploading...",
    uploadDefaultText: "",
    browseActionText: "Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel",
    notSpecifiedText: "<None Specified>",
    searchFilesPlaceHolderText: "Search Files...",
    searchFoldersPlaceHolderText: "Search Folders...",
    fileText: "File",
    folderText: "Folder"
};


