import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import FileUpload from "./FileUpload/FileUpload";
import utils from "../../../../utils";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";

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
                            //fileFormats={["image/png"]}
                            onSelectFile={this.onFileSelect.bind(this)} />
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