import React, {Component, PropTypes} from "react";
import { AddIcon, LinkIcon } from "dnn-svg-icons";
import Localization from "../../../localization";
import Table from "./Table";

class PageUrls extends Component {
    toggle (openId) {
    }
    
    render() {
        const {pageUrls, onEdit, onDelete} = this.props;
        
        const opened = false;
        
        return (
            /* eslint-disable react/no-danger */
            <div>
                <div className="AddItemRow">
                    <div className="link-icon" dangerouslySetInnerHTML={{ __html: LinkIcon }} />
                    <div className="sectionTitle">{Localization.get("UrlsForThisPage")}</div>
                    <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                        <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                        </div> {Localization.get("AddUrl")}
                    </div>
                </div>

                <Table pageUrls={pageUrls} onEdit={onEdit} onDelete={onDelete} />
            </div>
            /* eslint-enable react/no-danger */
        );
    }
}

PageUrls.propTypes = {
    pageUrls: PropTypes.arrayOf(PropTypes.object),
    onEdit: PropTypes.func,
    onDelete: PropTypes.func
};

export default PageUrls;