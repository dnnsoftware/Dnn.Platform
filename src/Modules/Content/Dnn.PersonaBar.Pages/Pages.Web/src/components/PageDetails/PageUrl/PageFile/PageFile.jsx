import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import FileUpload from "dnn-file-upload";
import utils from "../../../../utils";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";

class PageFile extends Component {

    onFileSelect(value) {
        this.props.onChangeField("fileIdRedirection", value.fileId);
        this.props.onChangeField("fileUrlRedirection", value.path);
    }

    render() {
        const {page} = this.props;
        const fileUrlRedirection = page.fileUrlRedirection;
        const utilities = {
            utilities: utils.getUtilities()
        };
        return (
            <div className={styles.pageFile}>
                <GridSystem>
                    <GridCell className="left-column">
                        <FileUpload
                            utils={utilities}
                            portalId={-1}
                            imagePath={fileUrlRedirection}
                            onImageSelect={this.onFileSelect.bind(this)} />
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