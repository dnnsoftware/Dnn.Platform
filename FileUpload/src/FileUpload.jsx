import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import LinkInput from "./LinkInput";
import Browse from "./Browse/Browse";
import UploadBar from "./UploadBar";

import "./style.less";

const Buttons = [
    { name: "browse", text: "Browse Filesystem" },
    { name: "upload", text: "Upload a File" },
    { name: "link", text: "Enter URL Link" }
];

const DefaultText = "Drag and Drop a File or Select an Option";
const onDragOverText = "Drag and Drop a File";

export default class FileUpload extends Component {
    constructor(props) {
        super(props);
        let fileExist = false;
        let selectedFile = null;
        let selectedFolder = null;
            
        this.state = {
            text: DefaultText,
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
        setTimeout(this.compareDimensions.bind(this), 2000);
    }

    componentWillMount() {
        const file = this.props.selectedFile;
        if (file) {
            const selectedFolder = { value: file.folderPath, key: file.folderId };
            const selectedFile = { value: file.fileName, key: file.fileId };
            const fileExist = true;
            this.setState({fileExist, selectedFile, selectedFolder}, () => {
                this.getPreviewUrl(file.fileId);
            });
        }
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
        this.setState({ text: DefaultText });
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
        const errorText = error && typeof error === "string" ? error : "Upload Failed";
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
                return this.handleError("wrong format");
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
        const componentDimension = ReactDOM.findDOMNode(this).getBoundingClientRect();

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
        sf.postfile(`UploadFromLocal${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}` , formData, this.uploadComplete.bind(this), this.handleError.bind(this));
        this.setState({ uploading: true, uploadComplete: false });
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
        this.setState({ draggedOver: true, text: onDragOverText });
    }

    onDragLeave() {
        this.setState({ draggedOver: false, text: DefaultText });
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

    render() {
        let buttons = Buttons;
        if (this.props.buttons) {
            buttons = buttons.filter((button) => {
                return this.props.buttons.some((propButton) => {
                    return button.name === propButton;
                });
            });
        }

        buttons = buttons.map((button) => {
            const svg = require(`!raw!./img/${button.name}.svg`);
            const isUpload = button.name === "upload";
            /* eslint-disable react/no-danger */
            const accept = this.props.fileFormats.join(",");
            return <div
                className={"button " + button.name}
                onMouseEnter={this.onMouseEnter.bind(this, button.text) }
                onMouseLeave={this.onMouseLeave.bind(this) }
                onClick={this.onButtonClick.bind(this, button.name) }
                key={button.name}>
                <div dangerouslySetInnerHTML={{ __html: svg }} />
                {isUpload && accept && <input type="file" accept={accept} onChange={this.onFileUpload.bind(this) } aria-label="File" />}
                {isUpload && !accept && <input type="file" onChange={this.onFileUpload.bind(this) } aria-label="File" />}
            </div>;
        });

        const buttonsStyle = { width: buttons.length * 67 };
        const src = this.state.fileUrl || "";
        const showImage = src && this.state.fileExist && !this.state.showLinkInput && !this.state.showFolderPicker;
        const className = "overlay" + (src && this.state.fileExist ? " has-image" : "") + (this.state.draggedOver ? " hover" : "");

        return <div className="dnn-file-upload">
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
                    <span>{this.state.text}</span>
                </div>

                {this.state.showLinkInput && <LinkInput
                    linkPath={this.state.linkPath}
                    onSave={this.uploadFromLink.bind(this) }
                    onCancel={this.hideFields.bind(this) }/>}
                {this.state.showFolderPicker && <Browse
                    utils={this.props.utils}
                    fileFormats={this.props.fileFormats}
                    selectedFolder={this.state.selectedFolder}
                    selectedFile={this.state.selectedFile}
                    onSave={this.onFileSelect.bind(this) }
                    onCancel={this.hideFields.bind(this) } />}
                {showImage && <div className="image-container">
                    <img
                        style={this.getImageStyle() }
                        onError={this.handleImageError.bind(this) }
                        src={src} alt="Image"/></div>}
                {this.state.selectedFile &&
                    <div className="dnn-file-upload-file-name"><span>{this.state.selectedFile.value}</span></div>}
            </div>
            {this.state.uploading && <UploadBar uploadComplete={this.state.uploadComplete} errorText={this.state.errorText} fileName={this.state.fileName}/>}
        </div>;
    }
}


FileUpload.propTypes = {
    //---REQUIRED PROPS---
    utils: PropTypes.object.isRequired,
    onSelectFile: PropTypes.func.isRequired,

    //---OPTIONAL PROPS---
    selectedFile: PropTypes.object,
    cropImagePreview: PropTypes.bool,
    buttons: PropTypes.array,
    folderName: PropTypes.string,
    portalId: PropTypes.number,
    fileFormats: PropTypes.array
};

FileUpload.defaultProps = {
    cropImagePreview: false,
    portalId: -1,
    fileFormats: []
};


