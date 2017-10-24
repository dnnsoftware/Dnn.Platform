import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import FileUpload from "dnn-file-upload";
import utils from "../../../../utils";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";
import Localization from "../../../../localization";

class PageFile extends Component {

    onFileSelect(selectedFile) {
        this.props.onChangeField("fileIdRedirection", selectedFile.fileId);
        this.props.onChangeField("fileFolderPathRedirection", selectedFile.folderPath);
        this.props.onChangeField("fileNameRedirection", selectedFile.fileName);
    }

    render() {
        const {page} = this.props;
        const utilities = {
            utilities: utils.getUtilities()
        };
        
        const selectedFile = page.fileIdRedirection ? {
            fileId: page.fileIdRedirection,
            folderPath: page.fileFolderPathRedirection,
            fileName: page.fileNameRedirection
        } : null;

        return (
            <div className={styles.pageFile}>
                <GridSystem>
                    <GridCell className="left-column">
                        <FileUpload
                            utils={utilities}
                            selectedFile={selectedFile}
                            onSelectFile={this.onFileSelect.bind(this)}
                            browseButtonText = {Localization.get("BrowseButton")}
                            uploadButtonText = {Localization.get("UploadButton")}
                            linkButtonText = {Localization.get("LinkButton")}
                            defaultText = {Localization.get("DragDefault")}
                            onDragOverText = {Localization.get("DragOver")}
                            uploadFailedText = {Localization.get("UploadFailed")}
                            wrongFormatText = {Localization.get("WrongFormat")}
                            imageText = {Localization.get("DefaultImageTitle")}
                            linkInputTitleText = {Localization.get("LinkInputTitle")}
                            linkInputPlaceholderText = {Localization.get("LinkInputPlaceHolder")}
                            linkInputActionText = {Localization.get("LinkInputAction")}
                            uploadCompleteText = {Localization.get("UploadComplete")}
                            uploadingText = {Localization.get("Uploading")}
                            uploadDefaultText = {Localization.get("UploadDefault")}
                            browseActionText = {Localization.get("BrowseAction")}
                            notSpecifiedText = {Localization.get("NotSpecified")}
                            searchFilesPlaceHolderText = {Localization.get("SearchFilesPlaceHolder")}
                            searchFoldersPlaceHolderText = {Localization.get("SearchFoldersPlaceHolder")}
                            fileText = {Localization.get("File")}
                            folderText = {Localization.get("Folder")}
                            />
                    </GridCell>
                    <GridCell className="right-column">
                        <PageUrlCommons {...this.props} display="vertical" />
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }
}

PageFile.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageFile;