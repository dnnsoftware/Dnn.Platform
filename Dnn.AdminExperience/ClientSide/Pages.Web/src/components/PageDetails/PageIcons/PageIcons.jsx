import React, {Component} from "react";
import PropTypes from "prop-types";
import { FileUpload, GridSystem, GridCell, Label } from "@dnnsoftware/dnn-react-common";
import Localization from '../../../localization';
import util from '../../../utils';
import styles from './styles.less';

export default class PageIcons extends Component {            
    constructor(props) {
        super(props);  
    }    

    onChangeIcon(key, fileInfo) {        
        const {onChangeField} = this.props;
        onChangeField(key, fileInfo);
    }

    render() {
        const {props} = this;  
        util.utilities = util.getUtilities();
        return (
            <div className={styles.pageIcons}>
                <GridSystem>
                    <GridCell className="left-column">
                        <Label 
                            tooltipMessage={Localization.get("IconFile.Help")}
                            label={Localization.get("IconFile")}                        
                        />
                        <FileUpload 
                            utils={util}
                            onSelectFile={this.onChangeIcon.bind(this, 'iconFile')}
                            selectedFile={props.page.iconFile}
                            folderName={props.page.iconFile ? props.page.iconFile.folderName: null}
                            fileFormats={["image/png", "image/jpg", "image/jpeg", "image/bmp", "image/gif", "image/svg+xml"]}
                            validationCode={props.validationCode}
                            browseActionText={Localization.get("BrowseAction")}                         // Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel
                            browseButtonText={Localization.get("BrowseButton")}                         // Browse Filesystem
                            defaultText={Localization.get("DragOver")}                                  // Drag and Drop a File                            
                            fileText={Localization.get("File")}                                         // File
                            folderText={Localization.get("Folder")}                                     // Folder
                            imageText={Localization.get("DefaultImageTitle")}                           // Image
                            linkButtonText={Localization.get("LinkButton")}                             // Enter URL Link
                            linkInputActionText={Localization.get("LinkInputAction")}                   // Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel
                            linkInputPlaceholderText={Localization.get("LinkInputPlaceholder")}         // http://example.com/imagename.jpg
                            linkInputTitleText={Localization.get("LinkInputTitle")}                     // URL Link
                            notSpecifiedText={Localization.get("NoneSpecified")}                        // <None Specified>
                            searchFilesPlaceHolderText={Localization.get("SearchFilesPlaceHolder")}                // Search Files...
                            searchFoldersPlaceHolderText={Localization.get("SearchFoldersPlaceholder")} // Search Folders...
                            uploadButtonText={Localization.get("UploadButton")}                         // Upload a File
                            uploadCompleteText={Localization.get("UploadComplete")}                     // Upload Complete
                            uploadingText={Localization.get("Uploading")}                               // Uploading...                            
                            uploadFailedText={Localization.get("UploadFailed")}                         // Upload Failed
                            wrongFormatText={Localization.get("WrongFormat")}                           // wrong format                            
                        />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label 
                            tooltipMessage={Localization.get("IconFileLarge.Help")}
                            label={Localization.get("IconFileLarge")}                        
                        />
                        <FileUpload 
                            utils={util}
                            onSelectFile={this.onChangeIcon.bind(this, 'iconFileLarge')}
                            selectedFile={props.page.iconFileLarge}
                            folderName={props.page.iconFileLarge ? props.page.iconFileLarge.folderName : null}
                            fileFormats={["image/png", "image/jpg", "image/jpeg", "image/bmp", "image/gif", "image/svg+xml"]}
                            validationCode={props.validationCode}
                            browseActionText={Localization.get("BrowseAction")}                         // Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel
                            browseButtonText={Localization.get("BrowseButton")}                         // Browse Filesystem
                            defaultText={Localization.get("DragOver")}                                  // Drag and Drop a File                            
                            fileText={Localization.get("File")}                                         // File
                            folderText={Localization.get("Folder")}                                     // Folder
                            imageText={Localization.get("DefaultImageTitle")}                           // Image
                            linkButtonText={Localization.get("LinkButton")}                             // Enter URL Link
                            linkInputActionText={Localization.get("LinkInputAction")}                   // Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel
                            linkInputPlaceholderText={Localization.get("LinkInputPlaceholder")}         // http://example.com/imagename.jpg
                            linkInputTitleText={Localization.get("LinkInputTitle")}                     // URL Link
                            notSpecifiedText={Localization.get("NoneSpecified")}                        // <None Specified>
                            searchFilesPlaceHolderText={Localization.get("SearchFilesPlaceHolder")}                // Search Files...
                            searchFoldersPlaceHolderText={Localization.get("SearchFoldersPlaceholder")} // Search Folders...
                            uploadButtonText={Localization.get("UploadButton")}                         // Upload a File
                            uploadCompleteText={Localization.get("UploadComplete")}                     // Upload Complete
                            uploadingText={Localization.get("Uploading")}                               // Uploading...                            
                            uploadFailedText={Localization.get("UploadFailed")}                         // Upload Failed
                            wrongFormatText={Localization.get("WrongFormat")}                           // wrong format                            
                        />
                    </GridCell>
                </GridSystem>
            </div>
        );
    }
}

PageIcons.propTypes = {
    page: PropTypes.object.isRequired,
    validationCode: PropTypes.string.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired
};